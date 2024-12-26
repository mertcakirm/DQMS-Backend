using Dapper;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace QDMS.Repositories
{
    public class FieldRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult<IEnumerable<DocumentFieldDBO>> GetDocumentFields(string documentId, string[]? shortNames = null)
        {
            try
            {
                IEnumerable<DocumentFieldDBO>? result = dbCon.Query<DocumentFieldDBO>(
                    $"SELECT * FROM `document-field` WHERE `documentId`=@docid AND `revisionId` IS NULL{(shortNames == null ? "" : " AND `shortName` IN @array")};", new
                    {
                        docid = documentId,
                        array = shortNames
                    });

                return DBResult<IEnumerable<DocumentFieldDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentFieldDBO>>.CreateFailed(ex);
            }
        }
        public DBResult<DocumentFieldDBO> GetDocumentField(string documentId, string name)
        {
            try
            {
                var result = dbCon.QueryFirstOrDefault<DocumentFieldDBO>(
                    $"SELECT * FROM `document-field` WHERE `documentId`=@docid AND `revisionId` IS NULL AND `shortName`=@name;", new
                    {
                        docid = documentId,
                        name
                    });

                return DBResult<DocumentFieldDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<DocumentFieldDBO>.CreateFailed(ex);
            }
        }
        public DBResult<IEnumerable<DocumentFieldDBO>> GetDocumentRevisionFields(string documentId, string revisionId)
        {
            try
            {
                IEnumerable<DocumentFieldDBO>? result = dbCon.Query<DocumentFieldDBO>(
                    $"SELECT * FROM `document-field` WHERE `documentId`=@docid AND `revisionId` = @revid;", new
                    {
                        docid = documentId,
                        revid = revisionId
                    });

                return DBResult<IEnumerable<DocumentFieldDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<DocumentFieldDBO>>.CreateFailed(ex);
            }
        }
        public DBResult<DocumentFieldDBO> GetDocumentField(string documentId)
                => GetDocumentFields(documentId).To(enumerable => enumerable.FirstOrDefault());

        public DBResult DeleteDocumentFields(string documentId, string[] shortNames, bool dontDeleteRevision = false)
        {
            try
            {
                int effectedRows = dbCon.Execute(
                    $"DELETE FROM `document-field` WHERE `documentId`=@docid AND `shortName` IN @array{(dontDeleteRevision ? " AND `revisionId` IS NULL" : "")};", new
                    {
                        docid = documentId,
                        array = shortNames
                    });

                return DBResult.Create(effectedRows == shortNames.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteDocumentRevisionFields(string documentId, string revid)
        {
            try
            {
                int effectedRows = dbCon.Execute(
                    $"DELETE FROM `document-field` WHERE `documentId`=@docid AND `revisionId` = @revid;", new
                    {
                        docid = documentId,
                        revid
                    });

                return DBResult.Create(effectedRows > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteAllDocumentFields(string documentId, bool dontDeleteRevision = false)
        {
            try
            {
                int effectedRows = dbCon.Execute(
                    $"DELETE FROM `document-field` WHERE `documentId`=@docid{(dontDeleteRevision ? " AND `revisionId` IS NULL" : "")};", new
                    {
                        docid = documentId
                    });

                return DBResult.Create(effectedRows > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult DeleteDocumentField(string documentId, string shortName) => DeleteDocumentFields(documentId, new string[] { shortName });

        public DBResult InsertDocumentFields(params DocumentFieldDBO[] fields)
        {
            try
            {
                var sql = "INSERT INTO `document-field` VALUES " +
                      string.Join(", ", fields.Select((_, i) => $"(@docid{i}, @sname{i}, @revid{i}, @value{i}, @hash{i})"));

                var parameters = new DynamicParameters();
                for (int i = 0; i < fields.Length; i++)
                {
                    parameters.Add($"@docid{i}", fields[i].DocumentId);
                    parameters.Add($"@sname{i}", fields[i].ShortName);
                    parameters.Add($"@revid{i}", fields[i].RevisionId);
                    parameters.Add($"@value{i}", fields[i].Value);
                    parameters.Add($"@hash{i}", fields[i].Hash);
                }

                int effectedRows = dbCon.Execute(sql, parameters);
                return DBResult.Create(effectedRows == fields.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }
        public DBResult InsertDocumentField(DocumentFieldDBO field) => InsertDocumentFields(field);
    }
}
