namespace QDMS.EmailTemplates.Other
{
    public class AjandaHatirlatma : IEmailTemplate
    {
        public AjandaHatirlatma(string etkinlikAdi, string tarihVeSaat, string aciklama)
        {
            EtkinlikAdi = etkinlikAdi;
            TarihVeSaat = tarihVeSaat;
            Aciklama = aciklama;
        }

        public string EtkinlikAdi { get; set; }
        public string TarihVeSaat { get; set; }
        public string Aciklama { get; set; }

        public string EmailTitle => "QDMS - Etkinlik Hatırlatma";

        public bool IsHtml => true;

        public string GetBody()
        {
            return $@"
        <h2 style=""text-align: center; color: #7459FF; margin-bottom: 20px;"">Etkinlik Hatırlatıcısı</h2>
        <p style=""text-align: center; font-size: 16px;"">
            <strong>Unutmayın!</strong> Planladığınız etkinliğiniz yaklaşıyor.
        </p>

        <div style=""background-color: #f4f4f9; border: 1px solid #e0e0e0; border-radius: 10px; padding: 20px; margin: 20px auto; max-width: 500px; box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);"">
            <p style=""margin: 10px 0; font-size: 18px;"">
                <strong>Etkinlik:</strong>
                <span style=""color: #7459FF;"">{EtkinlikAdi}</span>
            </p>
            <p style=""margin: 10px 0; font-size: 18px;"">
                <strong>Tarih & Saat:</strong>
                <span style=""color: #7459FF;"">{TarihVeSaat}</span>
            </p>
            <p style=""margin: 10px 0; font-size: 18px;"">
                <strong>Açıklama:</strong>
                <span>{Aciklama}</span>
            </p>
        </div>";
        }
    }
}
