using Dapper;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;

namespace QDMS.Repositories
{
    public class RoleRepository
    {
        private readonly MySqlConnection dbCon;
        private readonly ILogger logger;

        public RoleRepository(MySqlConnection dbCon, ILogger logger)
        {
            this.dbCon = dbCon;
            this.logger = logger;
        }

        public DBResult<IEnumerable<RoleDBO>> GetRoleGroups()
        {
            try
            {
                IEnumerable<RoleDBO>? result = dbCon.Query<RoleDBO>("SELECT * FROM `role`;");
                return DBResult<IEnumerable<RoleDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<RoleDBO>>.CreateFailed(ex);
            }
        }
        public DBResult<RoleDBO> GetRoleGroup(string? id)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `role` WHERE `id`=@id;";
                RoleDBO? result = dbCon.QueryFirstOrDefault<RoleDBO>(sqlQuery, new { id });
                return DBResult<RoleDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<RoleDBO>.CreateFailed(ex);
            }
        }
        public DBResult UpdateRoleGroup(string id, string name, ActionPerm perms)
        {
            try
            {
                string sqlQuery = $"UPDATE `role` SET `name`=@name, `permissions`=@perms WHERE `id`=@id;";
                bool success = dbCon.Execute(sqlQuery, new { id, name, perms = (long)perms }) > 0;

                return DBResult.Create(success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteRoleGroup(string id)
        {
            try
            {
                int effectedRows = dbCon.Execute("DELETE FROM `role` WHERE `id` = @id;", new { id });
                return DBResult.Create(effectedRows > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult CreateRoleGroup(RoleDBO newRoleGroup)
        {
            try
            {
                bool success = dbCon.Execute("INSERT INTO `role` VALUES(@id, @name, @perms);", new
                {
                    id = newRoleGroup.ID,
                    name = newRoleGroup.Name,
                    perms = (long)newRoleGroup.Permissions
                }) > 0;

                return DBResult.Create(success);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
    }
}
