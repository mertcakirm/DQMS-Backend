using QDMS.DTOs;

namespace QDMS.DBOs
{
    public class DocumentShareDBO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DocumentShareDBO()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        public DocumentShareDBO(CreateDocumentShareDTO dto, string docId, string sharedBy)
        {
            this.DocumentId = docId;
            this.UserId = dto.UserId;
            this.SharedBy = sharedBy;
            this.IsForReadOnly = dto.IsForReadOnly;
            this.Note = dto.Note;
        }

        public string DocumentId { get; set; }
        public string UserId { get; set; }
        public string SharedBy { get; set; }
        public bool IsForReadOnly { get; set; }
        public string? Note { get; set; }
    }
}
