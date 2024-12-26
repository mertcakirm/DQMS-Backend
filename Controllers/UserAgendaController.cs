using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QDMS.Classes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;
using QDMS.Services;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api/agenda")]
    public class UserAgendaController(AgendaRepository agendaRepository, EmailService emailService) : Controller
    {
        [HttpGet]
        public IActionResult GetMonthlyEvents([FromQuery, BindRequired] string year, [FromQuery, BindRequired] string month)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            int iYear = 0, iMonth = 0;

            if (!int.TryParse(year, out iYear) || !int.TryParse(month, out iMonth))
                return BadRequest();

            if (!agendaRepository.GetEvents(userId!, iYear, iMonth).TryGetValue(out var events))
                return StatusCode(500);

            return Ok(events);
        }

        [HttpPost]
        public IActionResult CreateEvent([FromBody] CreateAgendaEventDTO dto)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            var dbo = new AgendaEventDBO(dto, userId!);

            if (!agendaRepository.CreateEvent(dbo).IsSuccessful)
                return StatusCode(500);

            _ = emailService.ProcessCreationMails(new//direkt yollama
            {
                Type = "other_agenda",
                agendaEvent = dbo
            });

            return Ok(dbo.EventId);
        }

        [HttpPut]
        public IActionResult UpdateEvent([FromQuery(Name = "id"), BindRequired] string eventId, [FromBody] UpdateAgendaEventDTO dto)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!agendaRepository.UpdateEvent(eventId, dto).IsSuccessful)
                return NotFound("Maybe Bad Request");

            _ = Task.Run(async () =>
            {
                if (agendaRepository.GetEvent(userId!, eventId).TryGetValue(out var dbo))
                {
                    await emailService.ProcessUpdateMails(new
                    {
                        Type = "other_agenda",
                        agendaEvent = dbo
                    });
                }
            });

            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteEvent([FromQuery(Name = "id"), BindRequired] string eventId)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!agendaRepository.DeleteEvent(eventId).IsSuccessful)
                return NotFound();

            _ = Task.Run(async () =>
            {
                if (agendaRepository.GetEvent(userId!, eventId).TryGetValue(out var dbo))
                {
                    await emailService.ProcessDeletionMails(new
                    {
                        Type = "other_agenda",
                        agendaEvent = dbo
                    });
                }
            });

            return Ok();
        }
    }
}
