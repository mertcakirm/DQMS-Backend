using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.Metadata;

namespace QDMS.EmailTemplates
{
    public class ExternalDocumentChangedEmailTemplate : IEmailTemplate
    {
        private string DocumentId;
        private string DocumentTitle;
        private string UpdatedBy;

        public ExternalDocumentChangedEmailTemplate(string documentId, string documentTitle, string updatedBy)
        {
            DocumentId = documentId;
            DocumentTitle = documentTitle;
            UpdatedBy = updatedBy;
        }

        public string EmailTitle => "QDMS - Dışa bağlı döküman değişti";
        public bool IsHtml => true;

        public string GetBody()
        {
            return $@"
    <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
        <h2 style=""color: #444; font-size: 18px; margin-bottom: 15px;"">Evrak Değişikliği İnceleme Hatırlatması</h2>
        <p style=""font-size: 16px; margin-bottom: 10px;"">
            Aşağıdaki detaylara sahip dışa bağlı bir evrak değişikliği yapılmış olup incelemeniz beklenmektedir:
        </p>
        <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
            <p style=""margin: 5px 0;"">
                <strong>Sistem Numarası:</strong> {DocumentId}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Döküman Başlığı:</strong> {DocumentTitle}
            </p>
            <p style=""margin: 5px 0;"">
                <strong>Değiştiren Kullanıcı:</strong> {UpdatedBy}
            </p>
        </div>
        <p style=""margin-top: 15px; font-size: 16px;"">
            Lütfen evrağı gözden geçirerek gerekli aksiyonları alınız.
        </p>
    </div>";
        }

    }
}
