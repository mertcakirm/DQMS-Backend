namespace QDMS.EmailTemplates
{
    public class ExternalDocumentReminderEmailTemplate : IEmailTemplate
    {
            private string DocumentId;
            private string DocumentTitle;
            public ExternalDocumentReminderEmailTemplate(string documentId, string documentTitle)
            {
                DocumentId = documentId;
                DocumentTitle = documentTitle;
            }

            public string EmailTitle => "QDMS - Dışa bağlı dökümanın gözden geçirme tarihi geldi";
            public bool IsHtml => true;

            public string GetBody()
            {
                return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">Dışa bağlı döküman gözden geçirme hatırlatması</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıdaki detaylara sahip dışa bağlı bir dökümanın gözden geçirme tarihi geldi:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
        </div>
        <p style=""margin-top: 15px; font-size: 16px;"">
            Lütfen evrağı gözden geçirerek gerekli aksiyonları alınız.
        </p>
    </div>";
            
        }
    }
}
