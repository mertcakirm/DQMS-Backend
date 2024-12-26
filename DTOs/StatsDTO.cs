namespace QDMS.DTOs
{
    public class StatsDTO
    {
        public int UserCount { get; set; }
        public int m_RejectedRevisionReqCount { get; set; }
        public int m_AcceptedRevisionReqCount { get; set; }
        public int PendingRevisionRequestsCount { get; set; }
        public int m_AcceptedRevisionCount { get; set; }
        public int m_RejectedRevisionCount { get; set; }
        public int PendingRevisionsCount { get; set; }
        public IDictionary<string, int> DocumentCounts { get; set; }
        public IDictionary<string, int> m_DocumentCounts { get; set; }
        public IDictionary<string, int> m_DepartmentDocumentCounts { get; set; }
    }
}
