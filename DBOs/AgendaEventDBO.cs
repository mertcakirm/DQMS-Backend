using QDMS.Classes;
using QDMS.DTOs;

namespace QDMS.DBOs
{
    public class AgendaEventDBO
    {
        public AgendaEventDBO()
        {
        }

        public AgendaEventDBO(CreateAgendaEventDTO dto, string uid)
        {
            this.Uid = uid;
            this.EventId = CryptographyUtility.GenerateId(9);
            this.Date = dto.Date;
            this.Time = dto.Time;
            this.Title = dto.Title;
            this.Description = dto.Description;
            this.ColorIndex = dto.ColorIndex;
            this.Reminders = dto.Reminders;
        }

        public string Uid { get; set; }
        public string EventId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? Time { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public ushort ColorIndex { get; set; }
        public int Reminders { get; set; }
    }
}
