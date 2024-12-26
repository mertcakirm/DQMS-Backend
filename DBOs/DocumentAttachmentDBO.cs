namespace QDMS.DBOs
{
    public class DocumentAttachmentDBO
    {
        public DocumentAttachmentDBO()
        {
        }

        public DocumentAttachmentDBO(string attachmentID, string documentId, string name, string fileName, string extension, string? type)
        {
            AttachmentID = attachmentID;
            DocumentId = documentId;
            Name = name;
            FileName = fileName;
            Extension = extension;
            Type = type;
        }

        public string AttachmentID { get; set; }
        public string DocumentId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string? Type { get; set; }
        public int Reminders { get; set; }
    }
}
