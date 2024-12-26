using static System.Net.Mime.MediaTypeNames;

namespace QDMS.EmailTemplates
{
    public class ResetPasswordEmailTemplate : IEmailTemplate
    {
        private string ResetCode;

        public ResetPasswordEmailTemplate(string resetCode)
        {
            ResetCode = resetCode;
        }

        public string EmailTitle => "Şifre Sıfırlama Talebi";
        public bool IsHtml => true;

        public string GetBody()
        {
            return $@"<h2 class=""gtxt"">Şifre Sıfırlama,</h2>
                                 <p>Eğer şifre sıfırlama talebinde bulunmadıysanız lütfen bu maili dikkate almayınız.</p>
                            <code style=""margin: 10px 0px; padding: 10px 20px; border-radius: 12px; font-size: 35px; width: max-content; display: flex; justify-content: center; text-align: center; align-items: center;"">
                                {ResetCode}
                            </code>";
        }
    }
}