using QDMS.Classes;
using QDMS.DTOs;

namespace QDMS.DBOs
{
    public class DocumentFieldDBO
    {
        public DocumentFieldDBO()
        {
        }

        public DocumentFieldDBO(string documentId, string? revisionId, string shortName, string? value, string? hash)
        {
            DocumentId = documentId;
            RevisionId = revisionId;
            ShortName = shortName;
            Value = value;
            Hash = hash;
        }

        public string DocumentId { get; set; }
        public string? RevisionId { get; set; }
        public string ShortName { get; set; }
        public string? Value { get; set; }
        public string? Hash { get; set; }
    }
}
