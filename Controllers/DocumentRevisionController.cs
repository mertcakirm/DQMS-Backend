using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;
using QDMS.Services;
using System.Reflection.Metadata;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api/documents/{id:required:length(9)}/revision")]
    public class DocumentRevisionController : Controller
    {
        private readonly IConfiguration config;
        private readonly DocumentRepository documentRepository;
        private readonly FieldRepository fieldRepository;
        private readonly DocumentShareRepository documentShareRepository;
        private readonly RevisionRepository revisionRepository;
        private readonly EmailService emailService;

        public DocumentRevisionController(IConfiguration config, DocumentRepository documentRepository, FieldRepository fieldRepository, DocumentShareRepository documentShareRepository, RevisionRepository revisionRepository, EmailService emailService)
        {
            this.config = config;
            this.documentRepository = documentRepository;
            this.fieldRepository = fieldRepository;
            this.documentShareRepository = documentShareRepository;
            this.revisionRepository = revisionRepository;
            this.emailService = emailService;
        }

        [HttpPost]
        public IActionResult ReviseDocument([FromRoute(Name = "id")] string documentId,
                                            [FromBody] Dictionary<string, string?> fields)
        {
            var perms = HttpContext.GetPerms();
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            bool hasShare = documentShareRepository.GetDocumentShare(documentId, userId!).TryGetValue(out var shareDbo) && !shareDbo.IsForReadOnly;

            if (!perms.HasFlag(ActionPerm.DocumentRevisionCreate) && !hasShare)
                return Forbid();

            if (fields == null || fields.Count == 0)
                return BadRequest();

            var revId = CryptographyUtility.GenerateId(9);

            var res = revisionRepository.InsertRevision(new RevisionDBO(revId, documentId, userId!, RevisionState.Pending, null));
            var res1 = fieldRepository.InsertDocumentFields(fields.Select(kvp => new DocumentFieldDBO(documentId, revId, kvp.Key, kvp.Value, kvp.Value != null ? CryptographyUtility.ComputeSHA256Hash(kvp.Value!) : null)).ToArray());

            if (!res.IsSuccessful || !res1.IsSuccessful)
                return StatusCode(500);

            if (hasShare)
                documentShareRepository.DeleteDocumentShares(documentId, new string[] { userId! });

            if (documentRepository.GetDocument(documentId).TryGetValue(out var document))
                _ = emailService.SendNewRevisionMail(userId!, documentId, revId, document.Title!);

            return Ok();
        }

        [HttpPost("{rid:required:length(9)}/accept"), RequirePermission(ActionPerm.DocumentRevisionCreate)]
        public IActionResult AcceptRevision(string id, string rid)
        {
            if (!fieldRepository.GetDocumentRevisionFields(id, rid).TryGetValue(out var newFields))
                return NotFound();

            if (!revisionRepository.GetRevision(id, rid).TryGetValue(out var revisionDbo))
                return NotFound();

            if (revisionDbo.State != RevisionState.Pending)
                return StatusCode(406, "Revision status already changed!");

            fieldRepository.DeleteAllDocumentFields(id, true);
            fieldRepository.InsertDocumentFields(newFields.Select(f => { f.RevisionId = null; return f; }).ToArray());
            documentRepository.IncrementRevisionCount(id);
            //revisionRepository.DeleteRevision(rid);
            fieldRepository.DeleteDocumentRevisionFields(id, rid);

            if (documentRepository.GetDocument(id).TryGetValue(out var document))
                _ = emailService.ProcessDocumentUpdateMails(document, newFields?.ToArray() ?? Array.Empty<DocumentFieldDBO>());

            return Ok();
        }

        [HttpPost("{rid:required:length(9)}/reject"), RequirePermission(ActionPerm.DocumentRevisionCreate)]
        public IActionResult RejectRevision(string id, string rid, [FromBody] string? note)
        {
            if (!documentRepository.GetDocument(id).TryGetValue(out var document))
                return NotFound();

            if (!revisionRepository.GetRevision(id, rid).TryGetValue(out var revisionDbo))
                return NotFound();

            if (revisionDbo.State != RevisionState.Pending)
                return StatusCode(406, "Revision status already changed!");

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            var results = new DBResult[]
            {
                // Delete revision fields
                fieldRepository.DeleteDocumentRevisionFields(id, rid),
                // Share again
                documentShareRepository.InsertDocumentShares(new DocumentShareDBO[]
                {
                    new DocumentShareDBO(new CreateDocumentShareDTO()
                    {
                        IsForReadOnly = false,
                        Note = null,
                        UserId = revisionDbo.UserId
                    }, id, userId!)
                }),
                revisionRepository.SetRevision(rid, revDbo =>
                {
                    revDbo.State = RevisionState.Rejected;
                    revDbo.Note = note;
                }, revisionDbo),
            };

            _ = _ = emailService.SendRevisionRejectionMail(revisionDbo.UserId!, document.ID!, document.Title!, note);

            if (!results.All(r => r.IsSuccessful))
                return NotFound("Maybe error");

            return Ok();
        }

        [HttpGet("/api/documents/revisions/my")]
        public IActionResult GetSelfRevisions()
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!revisionRepository.GetRevisions(userId!).TryGetValue(out var result))
                return StatusCode(500);

            return Ok(result);
        }

        [HttpGet("/api/documents/revisions/{rid:required:length(9)}")]
        public IActionResult GetRevision(string rid)
        {
            if (!revisionRepository.GetRevisionWRevId(rid).TryGetValue(out var result))
                return StatusCode(500);

            if (!fieldRepository.GetDocumentRevisionFields(result.DocumentId, result.Id).TryGetValue(out var fields))
                return StatusCode(500);

            return Ok(new
            {
                revision = result,
                fields = fields.ToDictionary(f => f.ShortName)
            });
        }

        [HttpGet("/api/documents/revisions"), RequirePermission(ActionPerm.DocumentViewAll)]
        public IActionResult GetAllRevision([FromQuery(Name = "page")] string? strpage = "1", [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            if (!revisionRepository.GetAllRevision(page, max).TryGetValue(out var result))
                return StatusCode(500);

            return Ok(result);
        }

        // ------------- Revision Request -------------

        [HttpPost("request")]
        public IActionResult SendRevisionRequest(string id, [FromBody] string? note)
        {
            if (!documentRepository.GetDocument(id).TryGetValue(out var result))
                return NotFound();

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            var dbo = new RevisionRequestDBO(userId!, id, note);

            if (!revisionRepository.InsertRevisionRequest(dbo).IsSuccessful)
                return StatusCode(500);

            _ = emailService.SendNewRevisionRequestMail(userId!, result.ID!, dbo.Id, result.Title!, note);

            return Ok(dbo.Id);
        }

        [HttpGet("/api/documents/revision/requests/{reqid:required:length(9)}")]
        public IActionResult GetRevisionRequest(string reqid)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            var perms = HttpContext.GetPerms();

            if (!revisionRepository.GetRevisionRequest(reqid).TryGetValue(out var revReq))
                return NotFound();

            if (revReq.UserId != userId && !perms.HasFlag(ActionPerm.DocumentViewAll))
                return Forbid();

            return Ok(revReq);
        }

        [HttpDelete("/api/documents/revision/requests/{reqid:required:length(9)}")]
        public IActionResult DeleteRevisionRequest(string reqid)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            var perms = HttpContext.GetPerms();

            if (!revisionRepository.GetRevisionRequest(reqid).TryGetValue(out var revReq))
                return NotFound();

            if (revReq.UserId != userId && !perms.HasFlag(ActionPerm.DocumentDelete))
                return Forbid();

            if (!revisionRepository.DeleteRevisionRequests(reqid).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }

        [HttpGet("request/my")]
        public IActionResult GetMyRevisionRequests(string id,
                                            [FromQuery(Name = "page")] string? strpage = "1",
                                            [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!revisionRepository.GetRevisionRequests(page, max, uid: userId, docId: id).TryGetValue(out var results))
                return StatusCode(500);

            return Ok(results);
        }

        [HttpGet("/api/documents/revision/requests/my")]
        public IActionResult GetMyAllRevisionRequests([FromQuery(Name = "page")] string? strpage = "1",
                                               [FromQuery(Name = "max")] string? strmax = "10",
                                               [FromQuery(Name = "state")] string? stateStr = null)
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);


            Func<DBResult<IEnumerable<RevisionRequestDBO>>> f;

            if (!string.IsNullOrEmpty(stateStr) && Enum.TryParse<RevisionState>(stateStr, true, out var _state))
                f = new Func<DBResult<IEnumerable<RevisionRequestDBO>>>(() => revisionRepository.GetRevisionRequests(page, max, uid: userId, state: _state));
            else
                f = new Func<DBResult<IEnumerable<RevisionRequestDBO>>>(() => revisionRepository.GetRevisionRequests(page, max, uid: userId));

            if (!f().TryGetValue(out var results))
                return StatusCode(500);

            return Ok(results);
        }

        [HttpGet("requests"), RequirePermission(ActionPerm.DocumentViewAll)]
        public IActionResult GetAllRevisionRequestsForDocument(string id,
                                            [FromQuery(Name = "page")] string? strpage = "1",
                                            [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            if (!revisionRepository.GetRevisionRequests(page, max, docId: id).TryGetValue(out var results))
                return StatusCode(500);

            return Ok(results);
        }

        [HttpGet("/api/documents/revision/requests"), RequirePermission(ActionPerm.DocumentViewAll)]
        public IActionResult GetAllRevisionRequests([FromQuery, BindRequired] string statusType,
                                                    [FromQuery(Name = "page")] string? strpage = "1",
                                                    [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            if (!Enum.TryParse<RevisionState>(statusType, out var status))
                return BadRequest();

            if (!revisionRepository.GetRevisionRequests(page, max, state: status).TryGetValue(out var results))
                return StatusCode(500);

            return Ok(results);
        }

        [HttpPost("/api/documents/revision/requests/{reqid:required:length(9)}/accept"), RequirePermission(ActionPerm.DocumentRevisionCreate)]
        public IActionResult AcceptRevRequest(string reqid, [FromBody] string? note = null)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!revisionRepository.GetRevisionRequest(reqid).TryGetValue(out var revDbo))
                return NotFound();

            if (revDbo.Status != RevisionState.Pending)
                return StatusCode(406, "Status already changed");

            if (!revisionRepository.ChangeRevisionRequestState(reqid, RevisionState.Accepted).IsSuccessful)
                return NotFound();

            var res = documentShareRepository.InsertDocumentShares(new DocumentShareDBO[] {
                new DocumentShareDBO(new CreateDocumentShareDTO(revDbo.UserId, false, null), revDbo.DocumentId, userId!)
            });

            if (!res.IsSuccessful)
                return StatusCode(500);

            if (documentRepository.GetDocument(revDbo.DocumentId).TryGetValue(out var document))
                _ = emailService.SendRevisionRequestAcceptedMail(revDbo.UserId, revDbo.DocumentId, document.Title!, note);

            return Ok();
        }

        [HttpPost("/api/documents/revision/requests/{reqid:required:length(9)}/reject"), RequirePermission(ActionPerm.DocumentRevisionCreate)]
        public IActionResult RejectRevRequest(string reqid, [FromBody] string? note = null)
        {
            if (!revisionRepository.GetRevisionRequest(reqid).TryGetValue(out var revDbo))
                return NotFound();

            if (revDbo.Status != RevisionState.Pending)
                return StatusCode(406, "Status already changed");

            if (!revisionRepository.ChangeRevisionRequestState(reqid, RevisionState.Rejected).IsSuccessful)
                return NotFound();

            if (documentRepository.GetDocument(revDbo.DocumentId).TryGetValue(out var document))
                _ = emailService.SendRevisionRequestRejectionMail(revDbo.UserId, revDbo.DocumentId, document.Title!, note);

            return Ok();
        }
    }
}
