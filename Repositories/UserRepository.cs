using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using QDMS.Classes;
using QDMS.DBOs;
using QDMS.DTOs;

namespace QDMS.Repositories
{
    public class UserRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult<UserDBO> GetUser(string? username = null, string? email = null, string? uid = null, bool useOr = false)
        {
            try
            {
                string whereQuery = string.Empty;

                if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(username))
                    return DBResult<UserDBO>.Failed;

                var whereList = new List<string>();

                if (username != null)
                    whereList.Add("`username`=@username");

                if (email != null)
                    whereList.Add("`email`=@email");

                if (uid != null)
                    whereList.Add("`uid`=@uid");

                string sqlQuery = $"SELECT * FROM `user` WHERE {string.Join(useOr ? " OR " : " AND ", whereList)};";
                var @params = new { uid = uid ?? "", email = email ?? "", username = username ?? "" };

                var result = dbCon.QueryFirstOrDefault<UserDBO>(sqlQuery, @params);
                return DBResult<UserDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<UserDBO>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<UserDBO>> GetFirst100000Users()
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `user` LIMIT 100000;";
                var result = dbCon.Query<UserDBO>(sqlQuery);
                return DBResult<IEnumerable<UserDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<UserDBO>>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<string>> GetUserMailsForEmail(ActionPerm role, UserEmailPreference emailPref, out int rows, int page = 1, int max = 10, string[] excludeUID = null)
        {
            rows = 0;
            try
            {
                string sqlQuery = 
                                $@"
                                SELECT u.email FROM `user` u
                                INNER JOIN `role` r ON r.id = u.roleId
                                WHERE LENGTH(u.email) > 0 
                                AND (r.permissions & @permFlag) = @permFlag 
                                AND  (u.emailPreference & @prefFlag) = @prefFlag
                                AND u.uid NOT IN @exclude
                                LIMIT {(page - 1) * max}, {max};";

                excludeUID ??= Array.Empty<string>();

                var result = dbCon.Query<string>(sqlQuery, new
                {
                    permFlag = (uint)role,
                    prefFlag = (uint)emailPref,
                    exclude = excludeUID
                });
                return DBResult<IEnumerable<string>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<string>>.CreateFailed(ex);
            }
        }

        public DBResult ChangeUserPassword(string uid, string newPasswordHash)
        {
            try
            {
                string sqlQuery = $"UPDATE `user` SET `password`=@hash WHERE `uid`=@uid;";
                var result = dbCon.Execute(sqlQuery, new {uid, hash = newPasswordHash });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteUser(string uid)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `user` WHERE `uid`=@uid;";
                var result = dbCon.Execute(sqlQuery, new {uid});
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult InsertUser(UserDBO dbo)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `user` VALUES(@UID, @Username, @Password, @Name, @Surname, @Email, @RegDate, @Registerer, @RoleId, @EmailPreference);";
                var result = dbCon.Execute(sqlQuery, dbo);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult UpdateUser(ChangeUserDTO dto, string uid)
        {
            try
            {
                var list = new List<string>();

                if (dto.Username != null)
                    list.Add("username=@Username");

                if (dto.Name != null)
                    list.Add("name=@Name");

                if (dto.Surname != null)
                    list.Add("surname=@Surname");

                if (dto.Email != null)
                    list.Add("email=@Email");

                if (dto.RoleId != null)
                    list.Add("roleId=@RoleId");

                if (dto.EmailPref != null)
                    list.Add("emailPreference=@EmailPref");

                if (list.Count == 0)
                    return DBResult.CreateFailed();

                string sqlQuery = $"UPDATE `user` SET {string.Join(", ", list)} WHERE `uid`=@uid;";
                var result = dbCon.Execute(sqlQuery, new
                {
                    dto.Username,
                    dto.Name,
                    dto.Surname,
                    dto.Email,
                    dto.RoleId,
                    dto.EmailPref,
                    uid
                });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
    }
}
