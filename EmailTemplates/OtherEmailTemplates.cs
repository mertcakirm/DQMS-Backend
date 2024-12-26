using Microsoft.AspNetCore.Http.HttpResults;

namespace QDMS.EmailTemplates.Other
{
    public class HedefTakipHatirlatma : IEmailTemplate
    {
        private string DocumentId;
        private string Targets;
        private string Date;
        private string Remaining;

        public HedefTakipHatirlatma(string documentId, string targets, string date, string remaining)
        {
            DocumentId = documentId;
            Targets = targets;
            Date = date;
            Remaining = remaining;
        }

        public string EmailTitle => $"QDMS - Hedef için son {Remaining}";
        public bool IsHtml => true;

        public string GetBody()
        {
            return $@"
  <div style=""font-family: Arial, sans-serif; color: #333; line-height: 1.5; margin: 20px;"">
    <p style=""font-size: 16px; margin-bottom: 10px;"">
        Aşağıdaki detaylara sahip hedefin termin tarihine <strong>{Remaining}</strong> kaldı:
    </p>
    <div style=""border: 1px solid #ddd; padding: 15px; border-radius: 5px; background-color: #f9f9f9;"">
        <p style=""margin: 5px 0;"">
            <strong>Sistem Numarası:</strong> {DocumentId}
        </p>
        <p style=""margin: 5px 0;"">
            <strong>Termin Tarihi:</strong> {Date}
        </p>
        <p style=""margin: 5px 0;"">
            <strong>Kalan Süre:</strong> {Remaining}
        </p>
        <p style=""margin: 5px 0;"">
            <strong>Hedefler:</strong> {Targets}
        </p>
    </div>
    <p style=""margin-top: 15px; font-size: 16px;"">
        Lütfen revizyonu gözden geçirip gerekli işlemleri yapınız.
    </p>
</div>";
        }
    }
}
