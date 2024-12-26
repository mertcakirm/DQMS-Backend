using QDMS.DBOs;

namespace QDMS.DTOs
{
    public class CreateDocumentDTO
    {
        public string? Type { get; set; }
        public string? ShortName { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? ManuelId { get; set; }

        public Dictionary<string, string?>? Fields { get; set; }
    }
}
