using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;
using QDMS.Services;
using System.Text;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api/documents")]
    public class DocumentController : Controller
    {
        private readonly IConfiguration config;
        private readonly DocumentRepository documentRepository;
        private readonly FieldRepository fieldRepository;
        private readonly DocumentShareRepository documentShareRepository;
        private readonly FileService fileService;
        private readonly RevisionRepository revisionRepository;
        private readonly EmailService emailService;

        public DocumentController(IConfiguration config, DocumentRepository documentRepository, FieldRepository fieldRepository, DocumentShareRepository documentShareRepository, FileService fileService, RevisionRepository revisionRepository, EmailService emailService)
        {
            this.config = config;
            this.documentRepository = documentRepository;
            this.fieldRepository = fieldRepository;
            this.documentShareRepository = documentShareRepository;
            this.fileService = fileService;
            this.revisionRepository = revisionRepository;
            this.emailService = emailService;
        }

        [HttpPost, RequirePermission(ActionPerm.DocumentCreate)]
        public IActionResult CreateDocument([FromBody] CreateDocumentDTO document)
        {
            if (document == null
                || document.Fields == null
                || document.Fields.Count == 0
                || new string[] { document.ShortName, document.Title! }.Any(string.IsNullOrEmpty))
                return BadRequest();

            if (document.Fields.GroupBy(f => f.Key).Any(g => g.Count() > 1))//aynı field var mi
                return BadRequest();

            string? userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == CustomClaims.UserId)?.Value;//get userıd in jwt

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            DocumentDBO documentDBO = new DocumentDBO(document, userId);
            DocumentFieldDBO[] fieldsArray = new DocumentFieldDBO[document.Fields!.Count];

            for (int i = 0; i < fieldsArray.Length; i++)
            {
                var kvp = document.Fields.ElementAt(i);

                if (string.IsNullOrEmpty(kvp.Key))
                    return BadRequest();

                DocumentFieldDBO dbo = new DocumentFieldDBO(documentDBO.ID,
                                                            null, 
                                                            kvp.Key,
                                                            kvp.Value,
                                                            kvp.Value != null ? CryptographyUtility.ComputeSHA256Hash(kvp.Value!) : null);

                fieldsArray[i] = dbo;
            }

            var res1 = documentRepository.InsertDocument(documentDBO);

            if (!res1.IsSuccessful)
                return StatusCode(500);

            var res2 = fieldRepository.InsertDocumentFields(fieldsArray);

            if (!res2.IsSuccessful)
                return StatusCode(500);

            _ = emailService.ProcessDocumentCreationMails(documentDBO, fieldsArray);

            return Ok(documentDBO.ID);
        }

        [HttpGet("first"), RequirePermission(ActionPerm.DocumentViewAll)]
        public IActionResult GetFirstDocument([FromQuery(Name = "shortName"), BindRequired] string shortName)
        {
            var result = documentRepository.GetFirstDocument(shortName);

            if (!result.IsSuccessful)
                return NotFound();

            DocumentAttachmentDBO[] documentAttachments = Array.Empty<DocumentAttachmentDBO>();
            DocumentFieldDBO[]? documentFields = Array.Empty<DocumentFieldDBO>();

            if (documentRepository.GetDocumentAttachments(result.Value.ID!).TryGetValue(out var attachments))
                documentAttachments = attachments.ToArray();

            if (fieldRepository.GetDocumentFields(result.Value.ID!).TryGetValue(out var fields))
                documentFields = fields.ToArray();

            return Ok(new
            {
                Document = result.Value,
                Attachments = documentAttachments,
                Fields = documentFields.ToDictionary(f => f.ShortName)
            });
        }

        [HttpGet("multiple"), RequirePermission(ActionPerm.DocumentViewAll)]
        public IActionResult GetMultipleDocuments([FromQuery(Name = "shortName"), BindRequired] string shortName,
            [FromQuery(Name = "page")] string? strpage = "1", [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            var result = documentRepository.GetMultipleDocuments(shortName, page, max, out int rows);

            if (!result.IsSuccessful)
                return NotFound();

            return Ok(new { Rows = rows, Documents = result.Value });
        }

        [HttpGet]
        public IActionResult GetUserDocuments([FromQuery(Name = "page")] string? strpage = "1", 
                                              [FromQuery(Name = "max")] string? strmax = "10",
                                              [FromQuery] string? search = null,
                                              [FromQuery] List<string>? typeFilter = null,
                                              [FromQuery] List<string>? fields = null)
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            ActionPerm perms = HttpContext.GetPerms();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            DocumentDBO[] documents = Array.Empty<DocumentDBO>();
            int rows = 0;

            if (perms.HasFlag(ActionPerm.DocumentViewAll))
            {
                // Tüm dökümanlar
                var result = documentRepository.GetAllDocuments(page, max, out rows, search, typeFilter?.ToArray());

                if (!result.IsSuccessful)
                    return StatusCode(500);

                documents = result.Value!.ToArray();
            }
            else
            {
                // Kullanıcı dökümanları
                var result = documentRepository.GetAvailableDocumentsForUser(uid: userId, page, max, out rows, search, typeFilter?.ToArray());

                if (!result.IsSuccessful)
                    return StatusCode(500);

                documents = result.Value!.ToArray();
            }

                return Ok(new
                {
                    Rows = rows,
                    Documents = documents.Select(d =>
                    {
                        Dictionary<string, DocumentFieldDBO> dictionary = null;

                        if (fields != null && fields.Count > 0 && fieldRepository.GetDocumentFields(d.ID, fields.ToArray()).TryGetValue(out var array))
                            dictionary = array.ToDictionary(d => d.ShortName);

                        return new OutgoingDocumentDTO(d, dictionary);
                    })
                });
        }

        [HttpGet("my")]
        public IActionResult GetMyDocuments([FromQuery(Name = "page")] string? strpage = "1", [FromQuery(Name = "max")] string? strmax = "10")
        {
            int page = 1, max = 10;

            int.TryParse(strpage, out page);
            int.TryParse(strmax, out max);

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            ActionPerm perms = HttpContext.GetPerms();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            DocumentDBO[] documents = Array.Empty<DocumentDBO>();
            int rows = 0;

            // Kullanıcı dökümanları
            var result = documentRepository.GetAvailableDocumentsForUser(uid: userId, page, max, out rows, null, null);

            if (!result.IsSuccessful)
                return StatusCode(500);

            documents = result.Value!.ToArray();

            return Ok(new
            {
                Rows = rows,
                Documents = documents
            });
        }

        [HttpGet("{id:length(9):required}"), Authorize]
        public IActionResult GetUserDocument([FromRoute(Name = "id")] string documentId, [FromQuery] string? getFields = "true")
        {
            DocumentDBO documentDbo;

            if (!documentRepository.GetDocument(documentId, null, false).TryGetValue(out documentDbo))
                return NotFound();

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            bool hasShare = documentShareRepository.GetDocumentShare(documentId, userId).IsSuccessful;
            bool isOwnDocument = documentDbo.CreatorUID == userId;

            if (!hasShare && !HttpContext.GetPerms().HasFlag(ActionPerm.DocumentViewAll) && !isOwnDocument)
                return Forbid();

            DocumentAttachmentDBO[] documentAttachments = Array.Empty<DocumentAttachmentDBO>();
            DocumentFieldDBO[]? documentFields = Array.Empty<DocumentFieldDBO>();

            if (documentRepository.GetDocumentAttachments(documentId).TryGetValue(out var attachments))
                documentAttachments = attachments.ToArray();

            bool loadFields = true;
            bool.TryParse(getFields, out loadFields);

            if (loadFields && fieldRepository.GetDocumentFields(documentId).TryGetValue(out var fields))
                documentFields = fields.ToArray();

            return Ok(new
            {
                Document = documentDbo,
                Attachments = documentAttachments,
                Fields = documentFields.ToDictionary(f => f.ShortName)
            });
        }

        [HttpDelete("{id:length(9):required}"), RequirePermission(ActionPerm.DocumentDelete)]
        public IActionResult DeleteDocument([FromRoute(Name = "id")] string documentId)
        {
            var result = documentRepository.DeleteDocument(documentId);

            if (!result.IsSuccessful)
                return NotFound();

            documentRepository.DeleteAllDocumentAttachments(documentId);
            fieldRepository.DeleteAllDocumentFields(documentId);
            documentShareRepository.DeleteAllDocumentShares(documentId);
            revisionRepository.DeleteAllRevisions(documentId);
            fileService.DeleteDocumentAttachments(documentId);
            revisionRepository.DeleteRevisionRequests(docId: documentId);

            if (documentRepository.GetDocument(documentId).TryGetValue(out var doc))
                _ = emailService.ProcessDocumentDeletionMails(doc);

            return Ok();
        }

        [HttpPut("{id:length(9):required}"), RequirePermission(ActionPerm.DocumentModify)]
        public IActionResult ChangeDocumentFields([FromRoute(Name = "id")] string documentId,
                                                  [FromBody] Dictionary<string, string?> fields)
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (fields == null || fields.Count == 0)
                return BadRequest();

            if (!documentRepository.GetDocument(documentId).TryGetValue(out var document))
                return NotFound();

            fieldRepository.DeleteDocumentFields(documentId, fields.Keys.ToArray(), true);
            var fieldsArray = fields.Select(kvp => new DocumentFieldDBO(documentId, null, kvp.Key, kvp.Value, kvp.Value != null ? CryptographyUtility.ComputeSHA256Hash(kvp.Value!) : null)).ToArray();
            var res = fieldRepository.InsertDocumentFields(fieldsArray);

            documentRepository.IncrementRevisionCount(documentId);

            if (!res.IsSuccessful)
                return StatusCode(500);

            // --- Email ---
            if (document.Type! == DocumentType.ExternalDoc)
                _ = emailService.SendExternalDocumentChangedMail(userId!, documentId, document.Title!);

            if (document.Type! == DocumentType.ExternalDoc)
                _ = emailService.SendExternalDocumentDateMail(documentId, document.Title!, fieldsArray);

            if (document.Type! == DocumentType.ISGRisk)
                _ = emailService.SendISGRiskMail(documentId, document.Title!, fieldsArray);

            if (document.Type! == DocumentType.PurchaseRequestForm && fields.TryGetValue("field2", out string? fieldValue) && fieldValue != null)
                _ = emailService.SendPurchaseStateChangeMail(documentId, document.Title!, fieldValue);

            _ = emailService.ProcessDocumentUpdateMails(document, fieldsArray);
            // -------------- 

            return Ok();
        }

        [HttpGet("{id:length(9):required}/fields")]
        public IActionResult GetField(string id, [FromQuery, BindRequired] string fieldName)
        {
            DocumentDBO documentDbo;

            if (!documentRepository.GetDocument(id, null, false).TryGetValue(out documentDbo))
                return NotFound();

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            bool hasShare = documentShareRepository.GetDocumentShare(id, userId).IsSuccessful;
            bool isOwnDocument = documentDbo.CreatorUID == userId;

            if (!hasShare && !HttpContext.GetPerms().HasFlag(ActionPerm.DocumentViewAll) && !isOwnDocument)
                return Forbid();

            if (!fieldRepository.GetDocumentField(id, fieldName).TryGetValue(out var fieldValue))
                return NotFound();

            return Ok(fieldValue);
        }
    }
}