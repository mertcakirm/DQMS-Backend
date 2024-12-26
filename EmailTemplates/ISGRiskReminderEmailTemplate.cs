namespace QDMS.EmailTemplates
{
    public class ISGRiskReminderEmailTemplate : IEmailTemplate
    {
        private string DocumentId;
        private string DocumentTitle;
        public ISGRiskReminderEmailTemplate(string documentId, string documentTitle)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
        }

        public string EmailTitle => "QDMS - İSG Risk Değerlendirmesinin termin süresi yaklaşıyor";
        public bool IsHtml => true;

        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">İSG Risk Hatırlatma</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıda detayları verilen İSG Risk Değerlendirmesinin termin süresi yaklaşıyor:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
        </div>
    </div>";

        }
    }
}
