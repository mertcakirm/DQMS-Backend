using Dapper;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;

namespace QDMS.Repositories
{
    public class EmailRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult<PlannedEmailDBO> GetAndDeleteEmail()
        {
            try
            {
                string selectQuery = @"
            SELECT * FROM `email-planned` 
            WHERE 
                (YEAR(`date`) = @Year AND 
                MONTH(`date`) = @Month AND 
                DAY(`date`) = @Day AND 
                HOUR(`date`) = @Hour AND 
                MINUTE(`date`) = @Minute)
                OR `date` <= @Date
            ORDER BY `date` ASC LIMIT 1;";

                DateTime utcNow = DateTime.UtcNow;
                var parameters = new
                {
                    utcNow.Year,
                    utcNow.Month,
                    utcNow.Day,
                    utcNow.Hour,
                    utcNow.Minute,
                    Date = utcNow
                };

                PlannedEmailDBO? result = dbCon.QueryFirstOrDefault<PlannedEmailDBO>(selectQuery, parameters);

                if (result == null)
                    return DBResult<PlannedEmailDBO>.CreateIfNotNull(null);

                string deleteQuery = "DELETE FROM `email-planned` WHERE `id` = @Id";
                dbCon.Execute(deleteQuery, new { result.Id });

                return DBResult<PlannedEmailDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult<PlannedEmailDBO>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<PlannedEmailDBO>> GetUserSentEmails()
        {
            try
            {
                string selectQuery = @"SELECT * FROM `email-planned` WHERE data = 'U' ORDER BY `date` ASC;";
                var result = dbCon.Query<PlannedEmailDBO>(selectQuery);

                return DBResult<IEnumerable<PlannedEmailDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult<IEnumerable<PlannedEmailDBO>>.CreateFailed(ex);
            }
        }

        public DBResult DeleteEmail(string id)
        {
            try
            {
                string selectQuery = @"DELETE FROM `email-planned` WHERE id = @id;";
                var result = dbCon.Execute(selectQuery, new {id});

                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteEmailByData(string data)
        {
            try
            {
                string deleteQuery = "DELETE FROM `email-planned` WHERE `data` = @data";
                var res = dbCon.Execute(deleteQuery, new { data});

                return DBResult.Create(res > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteEmailByData(Func<string> func) => DeleteEmailByData(func());

        public DBResult CreateEmail(PlannedEmailDBO dbo)
        {
            try
            {
                string deleteQuery = "INSERT INTO `email-planned` VALUES(@Id, @Date, @Recipient, @Title, @IsHtml, @Body, @Data);";
                var res = dbCon.Execute(deleteQuery, dbo);

                return DBResult.Create(res > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult CreateEmails(IEnumerable<PlannedEmailDBO> dbos)
        {
            try
            {
                string insertQuery = "INSERT INTO `email-planned` VALUES (@Id, @Date, @Recipient, @Title, @IsHtml, @Body, @Data);";
                var res = dbCon.Execute(insertQuery, dbos);
                return DBResult.Create(res > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return DBResult.CreateFailed(ex);
            }
        }

    }
}
