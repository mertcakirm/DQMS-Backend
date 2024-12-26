using QDMS.DTOs;

namespace QDMS.DBOs
{
    public class RoleDBO
    {
        public RoleDBO(string? iD)
        {
            ID = iD;
        }

        public RoleDBO(string? iD, string? name, ActionPerm permissions)
        {
            ID = iD;
            Name = name;
            Permissions = permissions;
        }

        public string? ID { get; set; } = null;
        public string? Name { get; set; }
        public ActionPerm Permissions { get; set; }

        public RoleDBO Fill(UpdateRoleDTO dto)
        {
            this.Name = dto.Name;
            this.Permissions = dto.Permissions;
            return this;
        }
    }

    [Flags]
    public enum ActionPerm : long
    {
        None = 0,
        DocumentCreate = 2 << 0,
        DocumentDelete = 2 << 1,
        DocumentViewAll = 2 << 2,
        DocumentRevisionCreate = 2 << 3,
        DocumentAddUsers = 2 << 4,
        RoleManage = 2 << 5,
        DocumentModify = 2 << 6,
        UserDelete = 2 << 7,
        UserCreate = 2 << 8,
        UserModify = 2 << 9,
        SendMail = 2 << 10,
    }
}
