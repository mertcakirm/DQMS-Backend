using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QDMS.EmailTemplates;
using System.Collections.Concurrent;

namespace QDMS.Services
{
    public class EmailSenderService
    {
        private readonly string _baseHtml;
        private readonly int _emailProcessingInterval;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ConcurrentQueue<(IEmailTemplate EmailTemplate, string Recipient)> _emailQueue;
        private readonly Timer _timer;
        private readonly bool isUsable;

        private SmtpClient _client;
        private string _smtpSender;
        private string _smtpSenderName;

        public EmailSenderService(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            isUsable = unchecked(bool.Parse(_configuration["Smtp:IsUsable"]!));

            if (!isUsable)
                return;

            try
            {
                _emailQueue = new ConcurrentQueue<(IEmailTemplate, string)>();
                _baseHtml = File.ReadAllText(Path.Combine(_configuration["QDMS:ContentDirectory"]!, "templates", "emailbase.html"));
                _emailProcessingInterval = int.TryParse(_configuration["Smtp:EmailProcessingInterval"], out var interval) ? interval : 10_000;

                ConnectSmtp();

                _timer = new Timer(ProcessEmailQueue, null, _emailProcessingInterval, _emailProcessingInterval);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Error");
            }
        }

        private void ConnectSmtp()
        {
            if (!isUsable)
                return;

            try
            {
                _client?.Disconnect(true);
                _client?.Dispose();
            } catch
            {

            }

            var host = _configuration["Smtp:Host"]!;
            var port = int.Parse(_configuration["Smtp:Port"]!);
            var username = _smtpSender = _configuration["Smtp:Username"]!;
            var password = _configuration["Smtp:Password"]!;
            var socketOptions = (MailKit.Security.SecureSocketOptions)(int.Parse(_configuration["Smtp:SecureSocketOptions"]!));
            _smtpSenderName = _configuration["Smtp:SenderName"]!;

            _client = new SmtpClient();
            _client.Connect(host, port, socketOptions);
            _client.Authenticate(username, password);

            _logger.LogInformation("EmailSenderService initialized with SMTP Host: {Host}, Port: {Port}, Interval: {Interval}ms",
                host, port, _emailProcessingInterval);
        }

        public void AddEmailToQueue(string recipient, IEmailTemplate emailTemplate)
        {
            if (!isUsable)
                return;

            _emailQueue.Enqueue((emailTemplate, recipient));
        }

        private async void ProcessEmailQueue(object? state)
        {
            var tasks = new List<Task>();
            int maxEmailsToProcess = 1 /* 20 */;

            for (int i = 0; i < maxEmailsToProcess; i++)
            {
                if (_emailQueue.TryDequeue(out var email))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await SendEmailAsync(email.EmailTemplate, email.Recipient);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing email for recipient: {Recipient}", email.Recipient);
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }


        private async Task SendEmailAsync(IEmailTemplate emailTemplate, string recipient, int @try = 0)
        {
            try
            {
                string strBody = emailTemplate.GetBody();

                var message = new MimeMessage();
                var msgBodyBuilder = new BodyBuilder();

                if (emailTemplate.IsHtml)
                    msgBodyBuilder.HtmlBody = _baseHtml.Replace("{{{{%_%}}}}", strBody);
                else
                    msgBodyBuilder.TextBody = strBody;

                message.From.Add(new MailboxAddress(_smtpSenderName, _smtpSender));
                message.To.Add(new MailboxAddress("", recipient));
                message.Subject = emailTemplate.EmailTitle;
                message.Body = msgBodyBuilder.ToMessageBody();

                await _client.SendAsync(message);

#if DEBUG
                _logger.LogInformation($"Email sent to {recipient} in {@try + 1} try.");
#endif
            }
            catch (SmtpProtocolException authEx)
            {
                if (@try > 0)
                {
                    _logger.LogError(authEx, "Error sending email to {Recipient}", recipient);
                    return;
                }

                ConnectSmtp();
                await SendEmailAsync(emailTemplate, recipient, @try + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Recipient}", recipient);
            }
        }
    }
}
