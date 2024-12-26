namespace QDMS.EmailTemplates
{
    public class NewRevisionRequestEmailTemplate : IEmailTemplate
    {
        public string EmailTitle => "QDMS - Yeni revizyon talebi";
        public bool IsHtml => true;

        private string DocumentId;
        private string DocumentTitle;
        private string RevisionRequestId;
        private string CreatedBy;
        private string? Note;

        public NewRevisionRequestEmailTemplate(string documentId, string documentTitle, string revisionRequestId, string createdBy, string? note)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            RevisionRequestId = revisionRequestId;
            CreatedBy = createdBy;
            Note = note;
        }

        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">Yeni Bir Revizyon Oluşturuldu</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıdaki detaylara sahip yeni bir revizyon oluşturuldu:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Talep Numarası:</strong> {RevisionRequestId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Oluşturan Kullanıcı:</strong> {CreatedBy}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Not:</strong> {(string.IsNullOrWhiteSpace(Note) ? "Belirtilmedi" : $"<span style=\"color: #555;\">{Note}</span>")}
            </p>
        </div>
        <p style=""margin-top: 15px; font-size: 16px;"">
            Lütfen revizyonu gözden geçirip gerekli işlemleri yapınız.
        </p>
    </div>";
        }

    }
}
