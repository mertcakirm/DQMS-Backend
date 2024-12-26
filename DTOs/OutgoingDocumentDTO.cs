using QDMS.DBOs;

namespace QDMS.DTOs
{
    public class OutgoingDocumentDTO : DocumentDBO
    {
        public OutgoingDocumentDTO(DocumentDBO dbo, Dictionary<string, DocumentFieldDBO>? fields)
        {
            if (dbo != null)
            {
                ID = dbo.ID;
                Type = dbo.Type;
                ShortName = dbo.ShortName;
                Title = dbo.Title;
                CreatorUID = dbo.CreatorUID;
                CreationDate = dbo.CreationDate;
                PublishDate = dbo.PublishDate;
                RevisionCount = dbo.RevisionCount;
                Department = dbo.Department;
                ManuelId = dbo.ManuelId;
            }

            Fields = fields;
        }

        public Dictionary<string, DocumentFieldDBO> Fields { get; set; }
    }
}
