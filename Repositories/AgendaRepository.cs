using Dapper;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.CRUD;
using QDMS.Classes;
using QDMS.DBOs;
using QDMS.DTOs;

namespace QDMS.Repositories
{
    public class AgendaRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult CreateEvent(AgendaEventDBO dto)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `agenda-event` VALUES(@Uid, @EventId, @Date, @Time, @Title, @Description, @ColorIndex, @Reminders);";
                var result = dbCon.Execute(sqlQuery, dto);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult DeleteEvent(string eventId)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `agenda-event` WHERE eventId=@eventId;";
                var result = dbCon.Execute(sqlQuery, new {eventId});
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult DeleteEvents(string uid)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `agenda-event` WHERE uid=@uid;";
                var result = dbCon.Execute(sqlQuery, new { uid });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }


        public DBResult UpdateEvent(string eventId, UpdateAgendaEventDTO dto)
        {
            try
            {
                string sqlQuery = $"UPDATE `agenda-event` SET time=@Time, title=@Title, description=@Description, colorIndex=@ColorIndex, reminders=@Reminders WHERE eventId=@eventId;";
                var result = dbCon.Execute(sqlQuery, new 
                { 
                    eventId,
                    Time = dto.Time,
                    Title = dto.Title,
                    Description = dto.Description,
                    ColorIndex = dto.ColorIndex,
                    Reminders = dto.Reminders
                });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<AgendaEventDBO>> GetEvents(string uid, int year, int month)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `agenda-event` WHERE uid=@uid AND YEAR(date)=@year AND MONTH(date)=@month;";
                var result = dbCon.Query<AgendaEventDBO>(sqlQuery, new {uid, year, month});
                return DBResult<IEnumerable<AgendaEventDBO>>.CreateIfNotNull(result);  
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<AgendaEventDBO>>.CreateFailed(ex);
            }
        }

        public DBResult<AgendaEventDBO> GetEvent(string userId, string eventId)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `agenda-event` WHERE uid=@uid AND `eventId`=@id;";
                var result = dbCon.QueryFirstOrDefault<AgendaEventDBO>(sqlQuery, new { uid = userId, id = eventId });
                return DBResult<AgendaEventDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<AgendaEventDBO>.CreateFailed(ex);
            }
        }
    }
}
