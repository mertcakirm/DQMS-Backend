using QDMS.Classes;
using QDMS.EmailTemplates;

namespace QDMS.DBOs
{
    public class PlannedEmailDBO
    {
        public PlannedEmailDBO()
        {
        }

        public PlannedEmailDBO(DateTime date, string recipient, string? data)
        {
            Id = CryptographyUtility.GenerateId(9);
            Date = date;
            Recipient = recipient;
            Data = data;
        }

        public PlannedEmailDBO WithTitle(string title)
        {
            this.Title = title;
            return this;
        }
        public PlannedEmailDBO WithHtmlBody(string body)
        {
            this.IsHtml = true;
            this.Body = body;
            return this;
        }
        public PlannedEmailDBO WithTemplateBody(IEmailTemplate template)
        {
            this.IsHtml = template.IsHtml;
            this.Title = template.EmailTitle;
            this.Body = template.GetBody();
            return this;
        }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Recipient { get; set; }
        public string Title { get; set; }
        public bool IsHtml { get; set; }
        public string Body { get; set; }
        public string? Data { get; set; }
    }
}
