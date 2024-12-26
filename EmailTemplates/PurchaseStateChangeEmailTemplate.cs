namespace QDMS.EmailTemplates
{
    public class PurchaseStateChangeEmailTemplate : IEmailTemplate
    {
        public string EmailTitle => "QDMS - Satın alma durum bildirimi";
        public bool IsHtml => true;

        private string DocumentId;
        private string DocumentTitle;
        private string NewState;

        public PurchaseStateChangeEmailTemplate(string documentId, string documentTitle, string newState)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            NewState = newState;
        }

        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <p style=""font-size: 16px;"">
            Aşağıda detayları verilen satın alma dökümanının durumu değişti:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Yeni Durum:</strong> {NewState}
            </p>
        </div>
    </div>";
        }
    }
}
