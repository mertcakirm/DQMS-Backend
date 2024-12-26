namespace QDMS.EmailTemplates
{
    public class DynamicEmailTemplate : IEmailTemplate
    {
        private string title;
        private bool isHtml;
        private string body;

        public DynamicEmailTemplate(string title, bool isHtml, string body)
        {
            this.title = title;
            this.isHtml = isHtml;
            this.body = body;
        }

        public string EmailTitle => this.title;
        public bool IsHtml => this.isHtml;

        public string GetBody() => this.body;
    }
}
