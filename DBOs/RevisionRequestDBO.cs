using QDMS.Classes;

namespace QDMS.DBOs
{
    public class RevisionRequestDBO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RevisionRequestDBO()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        public RevisionRequestDBO(string userId, string documentId, string? note)
        {
            Id = CryptographyUtility.GenerateId(9);
            UserId = userId;
            DocumentId = documentId;
            Date = DateTime.UtcNow;
            Status = RevisionState.Pending;
            Note = note;
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string DocumentId { get; set; }
        public DateTime Date { get; set; }
        public RevisionState Status { get; set; }
        public string? Note { get; set; }

    }
}
