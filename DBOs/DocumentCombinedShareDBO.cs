namespace QDMS.DBOs
{
    public class DocumentCombinedShareDBO
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string ShortName { get; set; }
        public string Title { get; set; }
        public string CreatorUid { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public int RevisionCount { get; set; }
        public string Department { get; set; }
        public string ManuelId { get; set; }
        public string SharedBy { get; set; }
        public bool IsForReadOnly { get; set; }
        public string Note { get; set; }
    }

}
