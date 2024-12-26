using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.Repositories;
using QDMS.Services;
using System.Text;
using System.Reflection.Metadata;

namespace QDMS.Controllers
{
    [ApiController, Authorize, Route("api/documents/{id:length(9):required}/attachments")]
    public class DocumentAttachmentController : Controller
    {
        private readonly IConfiguration config;
        private readonly DocumentRepository documentRepository;
        private readonly FileService fileService;
        private readonly DocumentShareRepository documentShareRepository;

        public DocumentAttachmentController(IConfiguration config, DocumentRepository documentRepository, FileService fileService, DocumentShareRepository documentShareRepository)
        {
            this.config = config;
            this.documentRepository = documentRepository;
            this.fileService = fileService;
            this.documentShareRepository = documentShareRepository;
        }

        [HttpPost, RequirePermission(ActionPerm.DocumentCreate)]
        public async Task<IActionResult> UploadDocumentAttachment([FromRoute(Name = "id")] string documentId,
                                                      [FromQuery(Name = "extension"), BindRequired] string fileExtension,
                                                      [FromQuery(Name = "name"), BindRequired] string fileName,
                                                      [FromQuery(Name = "type")] string? type)
        {
            if (Request.ContentLength > Convert.ToInt64(config["qdms:MaxDocumentSizeKB"]) * 1024)
                return StatusCode(413); // Content Too Large

            byte[]? file = null;

            using (var ms = new MemoryStream())
            {
                await Request.Body.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                if (ms.Length == 0)
                    return BadRequest();

                file = Convert.FromBase64String(Encoding.UTF8.GetString(ms.ToArray()));
            }

            var documentResult = documentRepository.GetDocument(documentId);

            if (!documentResult.IsSuccessful)
                return NotFound();

            var attachmentDbo = new DocumentAttachmentDBO(CryptographyUtility.GenerateId(9), documentResult.Value.ID!, fileName, CryptographyUtility.GenerateId(20), fileExtension, type);

            if (!documentRepository.InsertDocumentAttachment(attachmentDbo).IsSuccessful)
                return StatusCode(500);

            if (!fileService.UploadDocumentAttachment(file, attachmentDbo.DocumentId, attachmentDbo.AttachmentID, attachmentDbo.Extension))
            {
                documentRepository.DeleteDocumentAttachment(attachmentDbo.DocumentId!, attachmentDbo.AttachmentID!);
                return StatusCode(500); // Internal Server Error
            }

            return Ok(attachmentDbo.AttachmentID);
        }

        [HttpDelete("{aid:required:length(9)}"), RequirePermission(ActionPerm.DocumentCreate)]
        public IActionResult DeleteDocumentAttachment([FromRoute(Name = "id")] string documentId,
                                                      [FromRoute(Name = "aid")] string attachmentId)
        {
            if (!documentRepository.DeleteDocumentAttachment(documentId, attachmentId).IsSuccessful)
                return NotFound();

            fileService.DeleteDocumentAttachment(documentId, attachmentId);

            return Ok();
        }

        [HttpGet("/api/documents/attachments/{aid:required:length(9)}")]
        public async Task<IActionResult> DownloadDocumentAttachment([FromRoute(Name = "aid")] string attachmentId,
                                                                    [FromQuery] string? b64 = "true")
        {
            if (!documentRepository.GetDocumentAttachment(attachmentId).TryGetValue(out var attachment))
                return NotFound("No Attachment");

            DocumentDBO documentDbo;

            if (!documentRepository.GetDocument(attachment.DocumentId).TryGetValue(out documentDbo))
                return NotFound("No Document");

            string? userId = HttpContext.GetClaim(CustomClaims.UserId);
            bool hasShare = documentShareRepository.GetDocumentShare(attachment.DocumentId, userId!).IsSuccessful;
            bool isOwnDocument = documentDbo.CreatorUID == userId;

            if (!hasShare && !HttpContext.GetPerms().HasFlag(ActionPerm.DocumentViewAll) && !isOwnDocument)
                return Forbid();

            var fileRes = fileService.GetDocumentAttachment(attachment.DocumentId, attachment.AttachmentID, attachment.Extension);

            if (fileRes == null)
                return NotFound("No File!");

            bool isB64 = true;
            bool.TryParse(b64, out isB64);

            if (isB64)
                return Ok(Convert.ToBase64String(fileRes));
            else
                return File(fileRes, "application/octet-stream");
        }
    }
}
