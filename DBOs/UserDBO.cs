using QDMS.Classes;
using QDMS.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace QDMS.DBOs
{
    public class UserDBO
    {
        public UserDBO(CreateUserDTO dto, string userId)
        {
            UID = CryptographyUtility.GenerateId(9);
            Registerer = userId;
            Username = dto.Username;
            Password = CryptographyUtility.ComputeSHA256Hash(dto.Password);
            Name = dto.Name;
            Surname = dto.Surname;
            Email = dto.Email;
            RegDate = DateTime.UtcNow;
            RoleId = dto.RoleId;
            EmailPreference = Enum.GetValues<UserEmailPreference>().Aggregate((a, b) => a | b);
        }
        public UserDBO()
        {
        }

        public string? UID { get; set; }

        /// <summary>
        /// UID
        /// </summary>
        public string? Registerer { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public DateTime RegDate { get; set; }
        public string? RoleId { get; set; }
        public UserEmailPreference EmailPreference { get; set; }

        public string FullName => Name + " " + Surname;
    }

    [Flags]
    public enum UserEmailPreference
    {
        None = 0,
        RevisionOpened = 1 << 0,
        RevisionRejected = 1 << 1,
        RevisionRequestOpened = 1 << 2,
        RevisionRequestAccepted = 1 << 3,
        RevisionRequestRejected = 1 << 4,
        DocumentShared = 1 << 5,
        ExternalDocChanged = 1 << 6,
        HedefPerformans = 1 << 7,
        ExternalDocReminder = 1 << 8,
        ISGRiskReminder = 1 << 9,
        PurchaseStateChangeMail = 1 << 10
    }
}
