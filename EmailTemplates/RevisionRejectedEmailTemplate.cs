namespace QDMS.EmailTemplates
{
    public class RevisionRejectedEmailTemplate : IEmailTemplate
    {
        private string DocumentId;
        private string DocumentTitle;
        private string? Reason;

        public RevisionRejectedEmailTemplate(string documentId, string documentTitle, string? reason)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            Reason = reason;
        }


        public string EmailTitle => "QDMS - Revizyonunuz reddedildi!";
        public bool IsHtml => true;
        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">Merhaba,</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıdaki detaylara sahip revizyonunuz <strong style=""color: #d9534f;"">reddedilmiştir</strong>:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Red Nedeni:</strong> {(string.IsNullOrWhiteSpace(Reason) ? "Belirtilmedi" : $"<span style=\"color: #555;\">{Reason}</span>")}
            </p>
        </div>
        <p style=""margin-top: 15px; font-size: 16px;"">
            Lütfen gerekli düzenlemeleri yaparak dökümanı tekrardan revizyona gönderin.
        </p>
    </div>";
        }
    }
}
