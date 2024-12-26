namespace QDMS.DBOs
{
    public class RevisionDBO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RevisionDBO()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        public RevisionDBO(string id, string documentId, string userId, RevisionState state, string? note)
        {
            Id = id;
            DocumentId = documentId;
            UserId = userId;
            State = state;
            Note = note;
            Date = DateTime.UtcNow;
        }

        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string UserId { get; set; }
        public RevisionState State { get; set; }
        public string? Note { get; set; }
        public DateTime? Date { get; set; }
    }

    public enum RevisionState : int
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2
    }
}
