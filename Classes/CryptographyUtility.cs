namespace QDMS.Classes
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class CryptographyUtility
    {
        public static string ComputeSHA256Hash(string input)
        {
            return ComputeSHA256Hash(Encoding.UTF8.GetBytes(input));
        }

        public static string GenerateId(int length)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length).ToUpper();
        }

        public static string ComputeSHA256Hash(byte[] data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
