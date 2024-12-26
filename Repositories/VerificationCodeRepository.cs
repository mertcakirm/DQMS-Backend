using Dapper;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;

namespace QDMS.Repositories
{
    public class VerificationCodeRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult CreateCode(VerificationCodeDBO code)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `user-verification` VALUES(@Id, @Uid, @Code);";
                var result = dbCon.Execute(sqlQuery, code);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult<VerificationCodeDBO> GetCode(string vid)
        {
            try
            {
                var result = dbCon.QueryFirstOrDefault<VerificationCodeDBO>("SELECT * FROM `user-verification` WHERE id=@vid", new {vid});
                return DBResult<VerificationCodeDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<VerificationCodeDBO>.CreateFailed(ex);
            }
        }

        public DBResult DeleteCode(string vid)
        {
            try
            {
                var result = dbCon.Execute("DELETE FROM `user-verification` WHERE id=@vid;", new { vid });
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
