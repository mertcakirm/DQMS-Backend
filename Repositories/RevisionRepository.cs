using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.DBOs;
using System.Collections.Generic;

namespace QDMS.Repositories
{
    public class RevisionRepository(MySqlConnection dbCon, ILogger logger)
    {
        public DBResult InsertRevision(RevisionDBO dbo)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `document-revision` VALUES(@Id, @DocumentId, @UserId, @State, @Note, @Date);";
                var result = dbCon.Execute(sqlQuery, dbo);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult DeleteRevision(string rid)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-revision` WHERE `id`=@rid;";
                var result = dbCon.Execute(sqlQuery, new { rid });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult DeleteAllRevisions(string documentId)
        {
            try
            {
                string sqlQuery = $"DELETE FROM `document-revision` WHERE `documentId`=@documentId;";
                var result = dbCon.Execute(sqlQuery, new { documentId });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult SetRevision(string rid, Action<RevisionDBO> func, RevisionDBO currentRev)
        {
            try
            {
                func(currentRev);

                string sqlQuery = $"UPDATE `document-revision` SET `state`=@state, `note`=@note WHERE `id`=@rid;";
                var result = dbCon.Execute(sqlQuery, new { rid, state = currentRev.State, note = currentRev.Note });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public int GetRevisionCount(string documentId)
        {
            try
            {
                string sqlQuery = $"SELECT COUNT(id) FROM `document-revision` WHERE `documentId`=@documentId;";
                int? result = dbCon.QueryFirstOrDefault<int?>(sqlQuery, new { documentId });
                return result ?? 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return 0;
            }
        }
        public DBResult<RevisionDBO> GetRevision(string documentId, string revid)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-revision` WHERE `documentId`=@documentId AND `id`=@revid;";
                var result = dbCon.QueryFirstOrDefault<RevisionDBO>(sqlQuery, new { documentId, revid });
                return DBResult<RevisionDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<RevisionDBO>.Failed;
            }
        }
        public DBResult<RevisionDBO> GetRevisionWRevId(string revid)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-revision` WHERE `id`=@revid;";
                var result = dbCon.QueryFirstOrDefault<RevisionDBO>(sqlQuery, new { revid });
                return DBResult<RevisionDBO>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<RevisionDBO>.Failed;
            }
        }
        public DBResult<IEnumerable<RevisionDBO>> GetAllRevision(int page, int max)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-revision` WHERE `state`=0 LIMIT {(page - 1) * max}, {max};";
                var result = dbCon.Query<RevisionDBO>(sqlQuery);
                return DBResult<IEnumerable<RevisionDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<RevisionDBO>>.Failed;
            }
        }
        public DBResult<IEnumerable<RevisionDBO>> GetRevisions(string uid)
        {
            try
            {
                string sqlQuery = $"SELECT * FROM `document-revision` WHERE `userId`=@uid;";
                var result = dbCon.Query<RevisionDBO>(sqlQuery, new { uid });
                return DBResult<IEnumerable<RevisionDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<RevisionDBO>>.Failed;
            }
        }

        /// <param name="id">Revision Request Id</param>
        public DBResult<RevisionRequestDBO> GetRevisionRequest(string id)
        {
            var result = GetRevisionRequests(1, 1, id: id);

            if (!result.IsSuccessful)
                return DBResult<RevisionRequestDBO>.Failed;

            var array = result.Value.ToArray();

            if (array.Length == 0)
                return DBResult<RevisionRequestDBO>.Failed;

            return DBResult<RevisionRequestDBO>.Create(array.First());
        }

        /// <param name="id">Revision Request ID</param>
        /// <param name="uid">User ID</param>
        /// <param name="docId">Document ID</param>
        public DBResult<IEnumerable<RevisionRequestDBO>> GetRevisionRequests(int page, int max, string? id = null, string? uid = null, string? docId = null, RevisionState? state = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(docId) && state == null)
                    return DBResult<IEnumerable<RevisionRequestDBO>>.Failed;

                var list = new List<string>();

                if (id != null)
                    list.Add("`id`=@id");

                if (uid != null)
                    list.Add("`userId`=@uid");

                if (docId != null)
                    list.Add("`documentId`=@docId");

                if (state != null)
                    list.Add("`status`=@state");

                string sqlQuery = $"SELECT * FROM `document-revision-request` WHERE {string.Join(" AND ", list)} ORDER BY `date` DESC LIMIT {(page - 1) * max}, {max};";
                var result = dbCon.Query<RevisionRequestDBO>(sqlQuery, new { id, uid, docId, state });
                return DBResult<IEnumerable<RevisionRequestDBO>>.CreateIfNotNull(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult<IEnumerable<RevisionRequestDBO>>.Failed;
            }
        }

        public DBResult InsertRevisionRequest(RevisionRequestDBO dbo)
        {
            try
            {
                string sqlQuery = $"INSERT INTO `document-revision-request` VALUES(@Id, @Date, @UserId, @DocumentId, @Status, @Note);";
                var result = dbCon.Execute(sqlQuery, dbo);
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        /// <param name="id">Revision Request Id</param>
        public DBResult DeleteRevisionRequests(string? id = null, string? uid = null, string? docId = null, RevisionState? state = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(docId) && state == null)
                    return DBResult.CreateFailed();

                var list = new List<string>();

                if (id != null)
                    list.Add("`id`=@id");

                if (uid != null)
                    list.Add("`userId`=@uid");

                if (docId != null)
                    list.Add("`documentId`=@docId");

                if (state != null)
                    list.Add("`status`=@state");

                string sqlQuery = $"DELETE FROM `document-revision-request` WHERE {string.Join(" AND ", list)};";
                var result = dbCon.Execute(sqlQuery, new { id, uid, docId, state });
                return DBResult.Create(result > 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Hata");
                return DBResult.CreateFailed(ex);
            }
        }

        public DBResult ChangeRevisionRequestState(string id, RevisionState newState)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return DBResult.CreateFailed();

                string sqlQuery = $"UPDATE `document-revision-request` SET `status`=@state WHERE `id`=@id;";
                var result = dbCon.Execute(sqlQuery, new { id, state = newState });
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
