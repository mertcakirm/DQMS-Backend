namespace QDMS.DTOs
{
    public class CreateAgendaEventDTO
    {
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public ushort ColorIndex { get; set; }
        public int Reminders { get; set; }
    }
    public class UpdateAgendaEventDTO
    {
        public TimeSpan? Time { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public ushort ColorIndex { get; set; }
        public int Reminders { get; set; }
    }
}
