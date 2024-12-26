using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;
using QDMS.EmailTemplates;
using QDMS.EmailTemplates.Other;
using QDMS.Repositories;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace QDMS.Services
{
    public class EmailService
    {
        private readonly UserRepository userRepository;
        private readonly EmailSenderService senderService;
        private readonly EmailRepository emailRepository;
        private readonly IConfiguration config;
        private readonly ILogger logger;
        private readonly Timer timer;

        public EmailService(EmailSenderService senderService, IConfiguration config, ILogger logger)
        {
            var mysqlCon = new MySqlConnection(config["MySql:ConnectionString"]);
            var mysqlCon1 = new MySqlConnection(config["MySql:ConnectionString"]);
            mysqlCon.Open();
            mysqlCon1.Open();

            this.senderService = senderService;
            this.config = config;
            this.logger = logger;
            this.userRepository = new UserRepository(mysqlCon, logger);
            this.emailRepository = new EmailRepository(mysqlCon1, logger);

            const int interval = 5_000;
            timer = new Timer(TimerCallback!, null, interval, interval);
            logger.LogInformation("EmailService initilized!");
        }

        private void TimerCallback(object obj)
        {
            int i = 0;
            while (++i < 50 && emailRepository.GetAndDeleteEmail().TryGetValue(out PlannedEmailDBO dbo))
            {
                if (DateTime.UtcNow - dbo.Date > TimeSpan.FromHours(12))
                    continue;

                senderService.AddEmailToQueue(dbo.Recipient, new DynamicEmailTemplate(dbo.Title, dbo.IsHtml, dbo.Body));
            }
        }

        public async Task SendRevisionRejectionMail(string toUID, string documentId, string documentTitle, string? reason)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var user) || string.IsNullOrEmpty(user.Email))
                return;

            if (user.EmailPreference.HasFlag(DBOs.UserEmailPreference.RevisionRejected))
                senderService.AddEmailToQueue(user.Email!, new EmailTemplates.RevisionRejectedEmailTemplate(documentId, documentTitle, reason));

            await Task.CompletedTask;
        }

        public async Task SendRevisionRequestRejectionMail(string toUID, string documentId, string documentTitle, string? reason)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var user) || string.IsNullOrEmpty(user.Email))
                return;

            if (user.EmailPreference.HasFlag(DBOs.UserEmailPreference.RevisionRequestRejected))
                senderService.AddEmailToQueue(user.Email!, new EmailTemplates.RevisionRequestEmailTemplate(documentId, documentTitle, reason, true));

            await Task.CompletedTask;
        }

        public async Task SendRevisionRequestAcceptedMail(string toUID, string documentId, string documentTitle, string? note)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var user) || string.IsNullOrEmpty(user.Email))
                return;

            if (user.EmailPreference.HasFlag(DBOs.UserEmailPreference.RevisionRequestAccepted))
                senderService.AddEmailToQueue(user.Email!, new EmailTemplates.RevisionRequestEmailTemplate(documentId, documentTitle, note, false));

            await Task.CompletedTask;
        }

        public async Task SendDocumentShareMail(string shareerUID, string toUID, string documentId, string documentTitle, bool isReadOnly, string? note)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var user) || string.IsNullOrEmpty(user.Email))
                return;

            if (!userRepository.GetUser(uid: shareerUID).TryGetValue(out var sharerUser))
                return;

            if (user.EmailPreference.HasFlag(DBOs.UserEmailPreference.DocumentShared))
                senderService.AddEmailToQueue(user.Email!,
                    new EmailTemplates.DocumentSharedEmailTemplate($"{sharerUser.FullName} (@{sharerUser.Username})", documentId, documentTitle, isReadOnly, note)); ;

            await Task.CompletedTask;
        }

        public async Task SendNewRevisionMail(string toUID, string documentId, string revid, string documentTitle)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var toUser))
                return;

            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll | DBOs.ActionPerm.DocumentRevisionCreate,
                           DBOs.UserEmailPreference.RevisionOpened,
                           out _, page: 1, max: 1_000);

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    senderService.AddEmailToQueue(mail, new NewRevisionEmailTemplate(documentId, revid, documentTitle, $"{toUser.FullName} (@{toUser.Username})"));
            }

            await Task.CompletedTask;
        }

        public async Task SendNewRevisionRequestMail(string toUID, string documentId, string reqid, string documentTitle, string? note)
        {
            if (!userRepository.GetUser(uid: toUID).TryGetValue(out var toUser))
                return;

            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll | DBOs.ActionPerm.DocumentRevisionCreate,
                           DBOs.UserEmailPreference.RevisionRequestOpened,
                           out _, page: 1, max: 1_000);

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    senderService.AddEmailToQueue(mail, new NewRevisionRequestEmailTemplate(documentId, documentTitle, reqid, $"{toUser.FullName} (@{toUser.Username})", note));
            }

            await Task.CompletedTask;
        }

        public async Task SendPurchaseStateChangeMail(string documentId, string documentTitle, string newState)
        {
            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll | DBOs.ActionPerm.DocumentRevisionCreate,
                           DBOs.UserEmailPreference.PurchaseStateChangeMail,
                           out _, page: 1, max: 1_000);

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    senderService.AddEmailToQueue(mail, new PurchaseStateChangeEmailTemplate(documentId, documentTitle, newState));
            }

            await Task.CompletedTask;
        }

        public async Task SendExternalDocumentChangedMail(string updaterUID, string documentId, string documentTitle)
        {
            if (!userRepository.GetUser(uid: updaterUID).TryGetValue(out var user))
                return;

            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll,
                           DBOs.UserEmailPreference.ExternalDocChanged,
                           out _, page: 1, max: 1_000);

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    senderService.AddEmailToQueue(mail, new ExternalDocumentChangedEmailTemplate(documentId, documentTitle, $"{user.FullName} (@{user.Username})"));
            }

            await Task.CompletedTask;
        }

        public async Task SendExternalDocumentDateMail(string documentId, string documentTitle, DocumentFieldDBO[] fields)
        {
            if (!fields.TryGetFirst(x => x.ShortName == "nextreview", out var field))
                return;

            DateTime? dt = null;

            try
            {
                dt = DateTime.ParseExact(field?.Value!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                return;
            }

            if (dt == null)
                return;

            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll,
                           DBOs.UserEmailPreference.ExternalDocReminder,
                           out _, page: 1, max: 1_000);

            var list = new List<PlannedEmailDBO>();

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    list.Add(new PlannedEmailDBO(dt.Value.ToUniversalTime() ,mail, null).WithTemplateBody(new ExternalDocumentReminderEmailTemplate(documentId, documentTitle)));
            }

            emailRepository.CreateEmails(list);

            await Task.CompletedTask;
        }

        public async Task SendISGRiskMail(string documentId, string documentTitle, DocumentFieldDBO[] fields)
        {
            if (!fields.TryGetFirst(x => x.ShortName == "deadline", out var field))
                return;

            DateTime? dt = null;

            try
            {
                dt = DateTime.ParseExact(field?.Value!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                return;
            }

            if (dt == null)
                return;

            var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll,
                           DBOs.UserEmailPreference.ISGRiskReminder,
                           out _, page: 1, max: 1_000);

            var list = new List<PlannedEmailDBO>();

            if (result.TryGetValue(out var emails))
            {
                foreach (string mail in emails)
                    list.Add(new PlannedEmailDBO(dt.Value.ToUniversalTime(), mail, null).WithTemplateBody(new ISGRiskReminderEmailTemplate(documentId, documentTitle)));
            }

            emailRepository.CreateEmails(list);

            await Task.CompletedTask;
        }

        public Task ProcessDocumentCreationMails(DocumentDBO document, DocumentFieldDBO[] fields)
        {
            return ProcessCreationMails(new
            {
                type = document.Type,
                document = document,
                fields = fields
            });
        }

        public Task ProcessDocumentDeletionMails(DocumentDBO document)
        {
            return ProcessDeletionMails(new
            {
                type = document.Type,
                document = document
            });
        }

        public Task ProcessDocumentUpdateMails(DocumentDBO document, DocumentFieldDBO[] fields)
        {
            return ProcessUpdateMails(new
            {
                type = document.Type,
                document = document,
                fields = fields
            });
        }

        public async Task ProcessCreationMails(dynamic data)
        {
            switch (data.type)
            {
                case DocumentType.Performance:
                    await this.__hedefPerformansSure(data.document, data.fields);
                    break;
                case "other_agenda":
                    AgendaEventDBO @event = data.agendaEvent;
                    await this.__ajandaHatirlatma(@event);
                    break;
                default:
                    await Task.CompletedTask;
                    break;
            }
        }
        public async Task ProcessDeletionMails(dynamic data)
        {
            switch (data.type)
            {
                case DocumentType.Performance:
                    emailRepository.DeleteEmailByData(data.document.ID);
                    break;
                case "other_agenda":
                    AgendaEventDBO @event = data.agendaEvent;
                    emailRepository.DeleteEmailByData(string.Concat(@event.Uid, "_", @event.EventId));
                    break;
                default:
                    await Task.CompletedTask;
                    break;
            }
        }
        public async Task ProcessUpdateMails(dynamic data)
        {
            switch (data.type)
            {
                case DocumentType.Performance:
                    emailRepository.DeleteEmailByData(data.document.ID);
                    this.__hedefPerformansSure(data.document, data.fields);
                    break;
                case "other_agenda":
                    AgendaEventDBO @event = data.agendaEvent;
                    emailRepository.DeleteEmailByData(string.Concat(@event.Uid, "_", @event.EventId));
                    await this.__ajandaHatirlatma(@event);
                    break;
                default:
                    await Task.CompletedTask;
                    break;
            }
        }

        private async Task __ajandaHatirlatma(AgendaEventDBO @event)
        {
            try
            {
                var map = new Dictionary<int, int>();
                map[1] = 0;
                map[2] = 10;
                map[4] = 30;
                map[8] = 60;
                map[16] = 1440;
                map[32] = 4320;

                if (userRepository.GetUser(uid: @event.Uid).TryGetValue(out var user))
                {
                    if (string.IsNullOrEmpty(user.Email))
                        return;

                    var list = new List<PlannedEmailDBO>();
                    DateTime utcDate = @event.Date.Date + @event.Date.TimeOfDay;

                    foreach (int minutes in map.Where(kvp => (@event.Reminders & kvp.Key) == kvp.Key).Select(kvp => kvp.Value))
                    {
                        var date = utcDate - TimeSpan.FromMinutes(minutes);
                        IEmailTemplate template = new EmailTemplates.Other.AjandaHatirlatma(@event.Title, utcDate./*ToLocalTime()*/ToString("dd/MM/yyyy HH:mm"), @event.Description ?? string.Empty);
                        list.Add(new PlannedEmailDBO(date, user.Email!, string.Concat(@event.Uid, "_", @event.EventId)).WithTemplateBody(template));
                    }

                    emailRepository.CreateEmails(list);
                }
            } catch
            {
                return;
            }

            await Task.CompletedTask;
        }

        private async Task __hedefPerformansSure(DocumentDBO document, DocumentFieldDBO[] fields)
        {
            try
            {
                // 'date1' -> Termin Tarihi
                if (!fields.TryGetAllFields(out var dFields, "date1", "field1"))
                    return;

                if (!DateTime.TryParse(dFields!["date1"]!.Value, out var dt))
                    return;

                var result = userRepository.GetUserMailsForEmail(
                           DBOs.ActionPerm.DocumentViewAll,
                           DBOs.UserEmailPreference.ExternalDocChanged,
                           out _, page: 1, max: 1_000);

                if (result.TryGetValue(out var emails))
                {
                    IEmailTemplate[] templates = {
                        new EmailTemplates.Other.HedefTakipHatirlatma(document.ID!, dFields!["field1"].Value!, dt.ToString("dd/MM/yyyy"), "1 ay"),
                        new EmailTemplates.Other.HedefTakipHatirlatma(document.ID!, dFields!["field1"].Value!, dt.ToString("dd/MM/yyyy"), "1 gün"),
                    };

                    emailRepository.CreateEmails(emails.SelectMany(email => new PlannedEmailDBO[]
                    {
                        new PlannedEmailDBO(dt.AddMonths(-1).ToUniversalTime(), email, document.ID!).WithTemplateBody(templates[0]),
                        new PlannedEmailDBO(dt.AddDays(-1).ToUniversalTime(), email, document.ID!).WithTemplateBody(templates[1]),
                    }));
                }

                await Task.CompletedTask;
            }
            catch
            {
                return;
            }
        }
    }

    static class ExtensionMethods
    {
        public static bool TryGetFirst<T>(this IEnumerable<T> array, Func<T, bool> pred, out T? outVar)
        {
            outVar = array.FirstOrDefault(pred);
            return outVar != null;
        }
        public static bool TryGetAllFields(this IEnumerable<DocumentFieldDBO> fields, out IDictionary<string, DocumentFieldDBO>? outFields, params string[] shortNames)
        {
            outFields = fields?.Where(f => shortNames.Any(sn => sn.Equals(f.ShortName)))?.ToDictionary(f => f.ShortName);
            return outFields != null && outFields.Keys.Count == shortNames.Length;
        }
    }
}
