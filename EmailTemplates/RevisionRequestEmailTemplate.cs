namespace QDMS.EmailTemplates
{
    public class RevisionRequestEmailTemplate : IEmailTemplate
    {
        private string DocumentId;
        private string DocumentTitle;
        private string? Reason;
        private bool IsRejection = true;

        public RevisionRequestEmailTemplate(string documentId, string documentTitle, string? reason, bool isRejection = true)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            Reason = reason;
            IsRejection = isRejection;
        }


        public string EmailTitle => $"QDMS - Revizyon talebiniz {(IsRejection ? "reddedilmiştir" : "onaylanmıştır")}";
        public bool IsHtml => true;
        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">Merhaba,</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıdaki detaylara sahip revizyon talebiniz 
            <strong style=""color: {(IsRejection ? "#d9534f" : "#5cb85c")};"">
                {(IsRejection ? "reddedilmiştir" : "onaylanmıştır")}
            </strong>:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>{(IsRejection ? "Red Nedeni" : "Not")}:</strong> 
                {(string.IsNullOrWhiteSpace(Reason) ? "Belirtilmedi" : $"<span style=\"color: #555;\">{Reason}</span>")}
            </p>
        </div>
    </div>";
        }
    }
}
