namespace QDMS.DTOs
{
    public class ManuelMailDTO
    {
        public string Title { get; set; }
        public string Recipient { get; set; }
        public bool IsHtml { get; set; }
        public string Body { get; set; }
        public DateTime? Date { get; set; } = null;
    }
}
