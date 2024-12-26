using QDMS.Classes;

namespace QDMS.DBOs
{
    public class VerificationCodeDBO
    {
        public VerificationCodeDBO()
        {
        }

        public VerificationCodeDBO(string uid)
        {
            Uid = uid;
            Code = CryptographyUtility.GenerateId(5);
            Id = CryptographyUtility.GenerateId(9);
        }

        public string Id { get; set; }
        public string Uid { get; set; }
        public string Code { get; set; }
    }
}
