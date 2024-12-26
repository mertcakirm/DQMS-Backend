using QDMS.DBOs;

namespace QDMS.DTOs
{
    public class ChangeUserDTO
    {
        public string? Username { get; set; } = null;
        public string? Name { get; set; } = null;
        public string? Surname { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? RoleId { get; set; } = null;
        public UserEmailPreference? EmailPref { get; set; } = null;
    }
}
