using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using QDMS.Classes;
using QDMS.DBOs;

namespace QDMS.Repositories
{
    public class DocumentRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult InsertDocument(DocumentDBO dbo)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `document` VALUES(@ID, @Type, @ShortName, @Title, @CreatorUID, @CreationDate, @PublishDate, 0, @Department, @ManuelId);";
                var result = dbCon.Execute(sqlQuery, dbo);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult<DocumentDBO> GetDocument(string id, string? shortName = null, bool useShortName = true)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document` WHERE `id`=@id{(useShortName ? $" AND `shortName`='{shortName ?? "document"}'" : "")};";
                var result = dbCon.QueryFirstOrDefault<DocumentDBO>(sqlQuery, new { id = id });
                return DBResult<DocumentDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentDBO>.CreateFailed(ex);
            }
        }
        public DBResult<DocumentDBO> GetFirstDocument(string shortName)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document` WHERE `shortName`=@shortName;";
                var result = dbCon.QueryFirstOrDefault<DocumentDBO>(sqlQuery, new { shortName });
                return DBResult<DocumentDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentDBO>.CreateFailed(ex);
            }
        }
        public DBResult<IEnumerable<DocumentDBO>> GetMultipleDocuments(string shortName, int page, int max, out int rows)
        {
            rows = 0;
            try
            {
                string baseQuery = $" FROM `document` WHERE `shortName`=@shortName LIMIT {(page - 1) * max}, {max};";

                string countQuery = $"SELECT COUNT(id)" + baseQuery;
                string normalQuery = $"SELECT *" + baseQuery;

                rows = dbCon.ExecuteScalar<int>(countQuery, new { shortName });

                var result = dbCon.Query<DocumentDBO>(normalQuery, new { shortName });
                return DBResult<IEnumerable<DocumentDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentDBO>>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<DocumentDBO>> GetUserDocuments(string uid, int page, int max, out int rows)
        {
            rows = 0;
            try
            {
                string sqlQuery = $@"
    SELECT COUNT(*) 
    FROM document 
    WHERE creatorUid = @uid AND `shortName` LIKE 'document%%';
    
    SELECT * 
    FROM document 
    WHERE creatorUid = @uid AND `shortName` LIKE 'document%%'
    LIMIT {(page - 1) * max}, {max};
";

                using (var multi = dbCon.QueryMultiple(sqlQuery, new { uid }))
                {
                    rows = multi.ReadSingle<int>();
                    var documents = multi.Read<DocumentDBO>();

                    return DBResult<IEnumerable<DocumentDBO>>.CreateIfNotNull(documents);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentDBO>>.CreateFailed(ex);
            }
        }
        public DBResult<IEnumerable<DocumentDBO>> GetAvailableDocumentsForUser(string uid, int page, int max, out int rows, string? search, string[]? typeFilter)
        {
            rows = 0;
            try
            {
                var conditions = new List<string> { "`shortName` LIKE 'document%%'", "(d.creatorUid = @uid OR ds.userId = @uid)" };

                if (search != null)
                    conditions.Add("(d.type LIKE @search OR d.title LIKE @search OR d.id LIKE @search OR d.department LIKE @search OR d.manuelId LIKE @search)");

                if (typeFilter != null)
                    conditions.Add("`type` IN @type");

                string whereQuery = string.Join(" AND ", conditions);
                string countQuery = $@"SELECT COUNT(DISTINCT d.id) FROM document d LEFT JOIN `document-share` ds 
                                      ON d.id = ds.documentId 
                                      WHERE {whereQuery};";

                var @params = new { uid, search = $"%{search}%", type = typeFilter };

                rows = dbCon.ExecuteScalar<int>(countQuery, @params);

                string dataQuery = $@"SELECT DISTINCT d.* FROM document d LEFT JOIN `document-share` ds ON d.id = ds.documentId 
                                      WHERE {whereQuery} ORDER BY d.creationDate DESC
                                      LIMIT {(page - 1) * max}, {max};";

                var result = dbCon.Query<DocumentDBO>(dataQuery, @params);
                return DBResult<IEnumerable<DocumentDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentDBO>>.CreateFailed(ex);
            }
        }

        public DBResult<IEnumerable<DocumentDBO>> GetAllDocuments(int page, int max, out int rows, string? search, string[]? typeFilter)
        {
            rows = 0;
            try
            {
                var conditions = new List<string> { "`shortName` LIKE 'document%%'" };

                if (search != null)
                    conditions.Add("(`type` LIKE @search OR `title` LIKE @search OR `id` LIKE @search OR `department` LIKE @search OR manuelId LIKE @search)");

                if (typeFilter != null)
                    conditions.Add("`type` IN @type");

                string whereQuery = string.Join(" AND ", conditions);
                string countQuery = $@"SELECT COUNT(id) FROM document WHERE {whereQuery};";

                var @params = new { search = $"%{search}%", type = typeFilter };

                rows = dbCon.ExecuteScalar<int>(countQuery, @params);

                string dataQuery = $@"SELECT * FROM document WHERE {whereQuery} ORDER BY `creationDate` DESC LIMIT {(page - 1) * max}, {max};";

                var result = dbCon.Query<DocumentDBO>(dataQuery, @params);
                return DBResult<IEnumerable<DocumentDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentDBO>>.CreateFailed(ex);
            }
        }

        public DBResult InsertDocumentAttachment(DocumentAttachmentDBO dbo)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `document-attachment` VALUES(@AttachmentId, @DocumentId, @Name, @FileName, @Extension, @Type);";
                var result = dbCon.Execute(sqlQuery, dbo);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteDocument(string documentId)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document` WHERE `id`=@documentId;";
                var result = dbCon.Execute(sqlQuery, new { documentId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult IncrementRevisionCount(string documentId)
        {
            try
            {
                string sqlQuery = $"UPDATE `document` SET `revisionCount`=`revisionCount`+1 WHERE `id`=@documentId;";
                var result = dbCon.Execute(sqlQuery, new { documentId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteDocumentAttachment(string docId, string aid)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-attachment` WHERE `attachmentId`=@aid AND `documentId`=@docid;";
                var result = dbCon.Execute(sqlQuery, new { aid, docId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteAllDocumentAttachments(string docId)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-attachment` WHERE `documentId`=@docid;";
                var result = dbCon.Execute(sqlQuery, new { docId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult<DocumentAttachmentDBO> GetDocumentAttachment(string docId, string aid)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-attachment` WHERE `attachmentId`=@aid AND `documentId`=@docid;";
                var result = dbCon.QueryFirstOrDefault<DocumentAttachmentDBO>(sqlQuery, new { aid, docId });
                return DBResult<DocumentAttachmentDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentAttachmentDBO>.CreateFailed(ex);
            }
        }
        public DBResult<DocumentAttachmentDBO> GetDocumentAttachment(string aid)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-attachment` WHERE `attachmentId`=@aid;";
                var result = dbCon.QueryFirstOrDefault<DocumentAttachmentDBO>(sqlQuery, new { aid });
                return DBResult<DocumentAttachmentDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentAttachmentDBO>.CreateFailed(ex);
            }
        }
        public DBResult<IEnumerable<DocumentAttachmentDBO>> GetDocumentAttachments(string docId)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-attachment` WHERE `documentId`=@docid;";
                var result = dbCon.Query<DocumentAttachmentDBO>(sqlQuery, new { docId });
                return DBResult<IEnumerable<DocumentAttachmentDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentAttachmentDBO>>.CreateFailed(ex);
            }
        }
    }
}
