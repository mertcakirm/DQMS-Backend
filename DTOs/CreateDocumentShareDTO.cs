namespace QDMS.DTOs
{
    public class CreateDocumentShareDTO
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CreateDocumentShareDTO()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        public CreateDocumentShareDTO(string userId, bool isForReadOnly, string? note)
        {
            UserId = userId;
            IsForReadOnly = isForReadOnly;
            Note = note;
        }

        public string UserId { get; set; }
        public bool IsForReadOnly { get; set; }
        public string? Note { get; set; }
    }
}
