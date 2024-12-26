using QDMS.Classes;
using QDMS.DTOs;

namespace QDMS.DBOs
{
    public class DocumentDBO
    {
        public string? ID { get; set; }
        public string? Type { get; set; }
        public string? ShortName { get; set; }
        public string? Title { get; set; }
        public string? CreatorUID { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public int RevisionCount { get; set; }
        public string? Department { get; set; }
        public string? ManuelId { get; set; }

        public DocumentDBO(CreateDocumentDTO dto, string uid)
        {
            ID = CryptographyUtility.GenerateId(9);
            Type = dto.Type;
            ShortName = dto.ShortName;
            Title = dto.Title;
            CreatorUID = uid;
            CreationDate = DateTime.UtcNow;
            PublishDate = null;
            RevisionCount = 0;
            Department = dto.Department;
            ManuelId = dto.ManuelId;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DocumentDBO()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }
    }
}
