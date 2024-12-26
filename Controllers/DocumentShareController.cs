using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;
using QDMS.Classes;
using QDMS.Services;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api/documents/{id:length(9):required}/share")]
    public class DocumentShareController : Controller
    {
        private readonly IConfiguration config;
        private readonly DocumentRepository documentRepository;
        private readonly DocumentShareRepository documentShareRepository;
        private readonly EmailService emailService;

        public DocumentShareController(IConfiguration config, DocumentRepository documentRepository, DocumentShareRepository documentShareRepository, EmailService emailService)
        {
            this.config = config;
            this.documentRepository = documentRepository;
            this.documentShareRepository = documentShareRepository;
            this.emailService = emailService;
        }

        [HttpGet]
        public IActionResult GetDocumentShares([FromRoute(Name = "id")] string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
                return BadRequest();

            var shareResult = documentShareRepository.GetDocumentShares(documentId);

            if (!shareResult.IsSuccessful)
                return StatusCode(500);

            return Ok(shareResult.Value);
        }

        [HttpGet("/api/documents/shared")]
        public IActionResult GetSharedDocuments([FromQuery(Name = "page")] string? strpage = "1",
                                                [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            var docsRes = documentShareRepository.GetSharedDocumentsForUser(userId!, page, max, out var rows);

            if (!docsRes.IsSuccessful)
                return StatusCode(500);

            return Ok(new
            {
                Rows = rows,
                Documents = docsRes.Value
            });
        }

        [HttpDelete, RequirePermission(ActionPerm.DocumentAddUsers)]
        public IActionResult DeleteDocumentShares([FromRoute(Name = "id")] string documentId,
                                                  [FromQuery, BindRequired] List<string> users)
        {
            if (string.IsNullOrEmpty(documentId))
                return BadRequest();

            var result = documentShareRepository.DeleteDocumentShares(documentId, users.ToArray());

            if (!result.IsSuccessful && result.IsError)
                return StatusCode(500);

            return Ok();
        }

        [HttpPost, RequirePermission(ActionPerm.DocumentAddUsers)]
        public IActionResult CreateDocumentShares([FromRoute(Name = "id")] string documentId,
                                                  [FromBody] CreateDocumentShareDTO[] dtos)
        {
            if (string.IsNullOrEmpty(documentId) || dtos == null || dtos.Length == 0)
                return BadRequest();

            string userId = HttpContext.GetClaim(CustomClaims.UserId) ?? string.Empty;
            var perms = HttpContext.GetPerms();

            List<DocumentShareDBO> dbos = new List<DocumentShareDBO>();

            foreach (var dto in dtos)
            {
                if (!dto.IsForReadOnly && !perms.HasFlag(ActionPerm.DocumentRevisionCreate))
                    return Forbid();

                dbos.Add(new DocumentShareDBO(dto, documentId, userId));
            }

            var result = documentShareRepository.InsertDocumentShares(dbos.ToArray());

            if (!result.IsSuccessful)
                return BadRequest();

            if (documentRepository.GetDocument(documentId).TryGetValue(out var docDbo))
                foreach (var dto in dtos)
                {
                    _ = emailService.SendDocumentShareMail(userId, dto.UserId, docDbo.ID!, docDbo.Title!, dto.IsForReadOnly, dto.Note);
                }

            return Ok();
        }
    }
}
