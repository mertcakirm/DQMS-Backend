using System.Buffers;

namespace QDMS.Services
{
    using Google.Protobuf;
    using System.Collections.Concurrent;
    using System.Net.Mail;

    public class FileService
    {
        private readonly IConfiguration config;
        private readonly ILogger logger;

        private static readonly ConcurrentDictionary<string, object> Locks = new();

        public FileService(IConfiguration config, ILogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        private object GetLockObject(string key, string? additional = null)
        {
            return Locks.GetOrAdd(key + (additional == null ? string.Empty : "_" + additional), _ => new object());
        }

        public bool UploadDocumentAttachment(byte[] file, string docId, string attachmentId, string extension)
        {
            try
            {
                string dirPath = Path.Combine(config["QDMS:ContentDirectory"]!, "documents", docId);

                lock (GetLockObject(dirPath))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    using (FileStream fs = new FileStream(Path.Combine(dirPath, $"{attachmentId}.{extension}"), FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        fs.Write(file, 0, file.Length);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return false;
            }
        }

        public byte[]? GetDocumentAttachment(string docId, string attachmentId, string extension)
        {
            try
            {
                string filePath = Path.Combine(config["QDMS:ContentDirectory"]!, "documents", docId, $"{attachmentId}.{extension}");

                if (!File.Exists(filePath))
                    return null;

                lock (GetLockObject(filePath))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        return buffer;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return null;
            }
        }

        public void DeleteDocumentAttachment(string docId, string attachmentId)
        {
            try
            {
                string folderPath = Path.Combine(config["QDMS:ContentDirectory"]!, "documents", docId);

                lock (GetLockObject(folderPath))
                {
                    string? filePath = Directory.GetFiles(folderPath).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == attachmentId);

                    if (filePath != null)
                        File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }
        }

        public void DeleteDocumentAttachments(string docId)
        {
            string folderPath = Path.Combine(config["QDMS:ContentDirectory"]!, "documents", docId);
            try
            {
                lock (GetLockObject(folderPath))
                {
                    if (Directory.Exists(folderPath))
                        Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }
            finally
            {
                Locks.TryRemove(folderPath, out _);
            }
        }

        public bool UploadUserPfp(string uid, string fileExt, byte[] buffer)
        {
            try
            {
                if (buffer == null)
                    return false;

                string dirPath = Path.Combine(config["QDMS:ContentDirectory"]!, "pfps");

                lock (GetLockObject(dirPath, uid))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    try
                    {
                        string? fDel = Directory.GetFiles(dirPath, "*.*").FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(uid));
                        if (fDel != null)
                            File.Delete(fDel);
                    }
                    catch { }

                    string filePath = Path.Combine(dirPath, $"{uid}.{fileExt}");
                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return false;
            }
        }
        public bool TryGetUserPfp(string uid, out byte[]? buffer, out string? fileExt)
        {
            buffer = null;
            fileExt = null;

            try
            {
                string folderPath = Path.Combine(config["QDMS:ContentDirectory"]!, "pfps");
                string? filePath = null;

                if (!Directory.Exists(folderPath))
                    return false;

                if ((filePath = Directory.GetFiles(folderPath, "*.*").FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(uid))) == null)
                    return false;

                fileExt = Path.GetExtension(filePath);

                lock (GetLockObject(folderPath, uid))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
                return false;
            }
        }
    }

}
