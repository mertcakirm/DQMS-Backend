namespace QDMS.EmailTemplates
{
    public class DocumentSharedEmailTemplate : IEmailTemplate
    {
        public string EmailTitle => "QDMS - Döküman paylaşımı";
        public bool IsHtml => true;

        private string Shareer;
        private string DocumentId;
        private string DocumentTitle;
        private bool IsReadOnly;
        private string? Note;

        public DocumentSharedEmailTemplate(string shareer, string documentId, string documentTitle, bool isReadOnly, string? note)
        {
            Shareer = shareer;
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            IsReadOnly = isReadOnly;
            Note = note;
        }

        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <p style=""font-size: 16px;"">
            <strong>{Shareer}</strong> kullanıcısı aşağıda detayları verilen dosyayı sizinle paylaştı:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Paylaşım Türü:</strong> {(IsReadOnly ? "Görüntüleme" : "Görüntüleme/Revizyon")}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Not:</strong> {(string.IsNullOrWhiteSpace(Note) ? "Belirtilmemiş" : $"<span style=\"color: #555;\">{Note}</span>")}
            </p>
        </div>
    </div>";
        }

    }
}
