using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection.Metadata;

namespace QDMS.EmailTemplates
{
    public class NewRevisionEmailTemplate : IEmailTemplate
    {
        public string EmailTitle => "QDMS - Yeni revizyon";
        public bool IsHtml => true;

        private string DocumentId;
        private string RevisionId;
        private string DocumentTitle;
        private string CreatedBy;

        public NewRevisionEmailTemplate(string documentId, string revisionId, string documentTitle, string createdBy)
        {
            DocumentId = documentId;
            RevisionId = revisionId;
            DocumentTitle = documentTitle;
            CreatedBy = createdBy;
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
             <strong>Revizyon Numarası:</strong> {RevisionId}
         </p>
         <p style=""margin: 5px 0;"">
             <strong>Döküman Başlığı:</strong> {DocumentTitle}
         </p>
         <p style=""margin: 5px 0;"">
             <strong>Oluşturan Kullanıcı:</strong> {CreatedBy}
         </p>
     </div>
     <p style=""margin-top: 15px; font-size: 16px;"">
         Lütfen revizyonu gözden geçirip gerekli işlemleri yapınız.
     </p>
 </div>";
        }
    }
}
