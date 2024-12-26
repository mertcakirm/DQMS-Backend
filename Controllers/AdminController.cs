using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DTOs;
using QDMS.EmailTemplates;
using QDMS.Repositories;
using QDMS.Services;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api")]
    public class AdminController(EmailSenderService emailSenderService, EmailRepository emailRepository, MySqlConnection dbCon) : Controller
    {
        [HttpGet("dashboard")]
        public IActionResult GetDashboard([FromQuery] DateTime? date = null)
        {
            DateTime dt = DateTime.UtcNow;

            var dto = new StatsDTO()
            {
                UserCount = dbCon.ExecuteScalar<int>("SELECT COUNT(uid) FROM user;"),
                PendingRevisionRequestsCount = dbCon.ExecuteScalar<int>("SELECT COUNT(id) FROM `document-revision-request` WHERE status = 0;"),
                m_RejectedRevisionReqCount = dbCon.ExecuteScalar<int>($"SELECT COUNT(id) FROM `document-revision-request` WHERE status = 2 AND YEAR(date) = {dt.Year} AND MONTH(date) = {dt.Month};"),
                m_AcceptedRevisionReqCount = dbCon.ExecuteScalar<int>($"SELECT COUNT(id) FROM `document-revision-request` WHERE status = 1 AND YEAR(date) = {dt.Year} AND MONTH(date) = {dt.Month};"),
                PendingRevisionsCount = dbCon.ExecuteScalar<int>("SELECT COUNT(id) FROM `document-revision` WHERE state = 0;"),
                m_RejectedRevisionCount = dbCon.ExecuteScalar<int>($"SELECT COUNT(id) FROM `document-revision` WHERE state = 2 AND YEAR(date) = {dt.Year} AND MONTH(date) = {dt.Month};"),
                m_AcceptedRevisionCount = dbCon.ExecuteScalar<int>($"SELECT COUNT(id) FROM `document-revision` WHERE state = 1 AND YEAR(date) = {dt.Year} AND MONTH(date) = {dt.Month};"),
                DocumentCounts = dbCon.Query("SELECT type, COUNT(*) AS DocumentCount  FROM document GROUP BY type;").ToDictionary(obj => (string)obj.type, obj1 => (int)obj1.DocumentCount),
                m_DocumentCounts = dbCon.Query($"SELECT type, COUNT(*) AS DocumentCount FROM document WHERE YEAR(creationDate) = {dt.Year} AND MONTH(creationDate) = {dt.Month} GROUP BY type;").ToDictionary(obj => (string)obj.type, obj1 => (int)obj1.DocumentCount),
                m_DepartmentDocumentCounts = dbCon.Query($"SELECT department, COUNT(*) AS DocumentCount FROM document WHERE YEAR(creationDate) = {dt.Year} AND MONTH(creationDate) = {dt.Month} AND LENGTH(department) > 0 GROUP BY department;").ToDictionary(obj => (string)obj.department, obj1 => (int)obj1.DocumentCount),
            };

            return Ok(dto);
        }

        [HttpPost("mails"), RequirePermission(DBOs.ActionPerm.SendMail)]
        public IActionResult SendManuelMail([FromBody] ManuelMailDTO dto)
        {
            IEmailTemplate template = new DynamicEmailTemplate(dto.Title, dto.IsHtml, dto.Body);
            string emailId = null;

            if (dto.Date == null)
                emailSenderService.AddEmailToQueue(dto.Recipient, template);
            else
            {
                var dbo = new DBOs.PlannedEmailDBO(dto.Date.Value, dto.Recipient, "U").WithTemplateBody(template);
                emailId = dbo.Id;
                emailRepository.CreateEmail(dbo);
            }


            return Ok(emailId);
        }

        [HttpGet("mails"), RequirePermission(DBOs.ActionPerm.SendMail)]
        public IActionResult GetScheduledMails()
        {
            if (!emailRepository.GetUserSentEmails().TryGetValue(out var result))
                return StatusCode(500);

            return Ok(result);
        }

        [HttpDelete("mails/{mid:required}"), RequirePermission(DBOs.ActionPerm.SendMail)]
        public IActionResult DeleteScheduledMails(string mid)
        {
            if (!emailRepository.DeleteEmail(mid).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }
    }
}
