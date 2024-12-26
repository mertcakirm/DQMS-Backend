using Dapper;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;
using System.Security.Cryptography.X509Certificates;

namespace QDMS.Repositories
{
    public class DocumentShareRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult<DocumentShareDBO> GetDocumentShare(string documentId, string userId)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-share` WHERE `documentId`=@documentId AND `userId`=@userId;";
                var result = dbCon.QueryFirstOrDefault<DocumentShareDBO>(sqlQuery, new { documentId, userId });
                return DBResult<DocumentShareDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentShareDBO>.CreateFailed(ex);
            }
        }
        public DBResult<IEnumerable<DocumentShareDBO>> GetDocumentShares(string documentId)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-share` WHERE `documentId`=@documentId;";
                var result = dbCon.Query<DocumentShareDBO>(sqlQuery, new { documentId });
                return DBResult<IEnumerable<DocumentShareDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentShareDBO>>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<DocumentCombinedShareDBO>> GetSharedDocumentsForUser(string uid, int page, int max, out int rows)
        {
            rows = 0;
            try
            {
                string baseQuery = "FROM `document` d INNER JOIN `document-share` ds ON ds.documentId = d.id WHERE d.`creatorUid`=@uid AND ds.userId=@uid";
                string countQuery = $"SELECT COUNT(*) {baseQuery};";
                string query = $@"
SELECT 
    d.id AS Id,
    d.type AS Type,
    d.shortName AS ShortName,
    d.title AS Title,
    d.creatorUid AS CreatorUid,
    d.creationDate AS CreationDate,
    d.publishDate AS PublishDate,
    d.revisionCount AS RevisionCount,
    d.department AS Department,
    d.manuelId AS ManuelId,
    ds.sharedBy AS SharedBy,
    ds.isForReadOnly AS IsForReadOnly,
    ds.note AS Note
{baseQuery} LIMIT {(page - 1) * max}, {max};";

                object @params = new
                {
                    uid
                };

                rows = dbCon.ExecuteScalar<int>(countQuery, @params);

                var result = dbCon.Query<DocumentCombinedShareDBO>(query, @params);
                return DBResult<IEnumerable<DocumentCombinedShareDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentCombinedShareDBO>>.CreateFailed(ex);
            }
        }

        public DBResult DeleteDocumentShares(string documentId, string[] userIds)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-share` WHERE `documentId`=@documentId AND `userId` IN @users;";
                var result = dbCon.Execute(sqlQuery, new { documentId, users = userIds });
                return DBResult.Create(result == userIds.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteAllDocumentShares(string documentId)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-share` WHERE `documentId`=@documentId;";
                var result = dbCon.Execute(sqlQuery, new { documentId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult InsertDocumentShares(DocumentShareDBO[] dbos)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `document-share` VALUES " +
                    string.Join(", ", dbos.Select((_, i) => $"(@DocumentId{i}, @UserId{i}, @SharedBy{i}, @IsForReadOnly{i}, @Note{i})")) + ";";

                var parameters = new DynamicParameters();
                for (int i = 0; i < dbos.Length; i++)
                {
                    parameters.Add($"@DocumentId{i}", dbos[i].DocumentId);
                    parameters.Add($"@UserId{i}", dbos[i].UserId);
                    parameters.Add($"@SharedBy{i}", dbos[i].SharedBy);
                    parameters.Add($"@IsForReadOnly{i}", dbos[i].IsForReadOnly);
                    parameters.Add($"@Note{i}", dbos[i].Note);
                }

                var result = dbCon.Execute(sqlQuery, parameters);
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
