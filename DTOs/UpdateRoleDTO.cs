using QDMS.DBOs;

namespace QDMS.DTOs
{
    public class UpdateRoleDTO
    {
        public string? Name { get; set; }
        public ActionPerm Permissions { get; set; }
    }
}
