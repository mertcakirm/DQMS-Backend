using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;
using QDMS.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace QDMS.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserController(IConfiguration config, EmailSenderService emailService, UserRepository userRepository, RoleRepository roleRepository, FileService fileService, AgendaRepository agendaRepository, VerificationCodeRepository verCodeRepository) : Controller
    {
        [HttpPost("self/login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginDTO loginData)
        {
            var userResult = userRepository.GetUser(username: loginData.Username);

            if (userResult.IsError)
                return StatusCode(500);

            // User not found in database
            if (!userResult.IsSuccessful)
                return NotFound(userResult.Exception?.ToString() ?? "");

            var user = userResult.Value;

            // Compute hash for password
            string computedHash = CryptographyUtility.ComputeSHA256Hash(loginData.Password ?? "");

            // Check password hash
            if (user.Password != computedHash)
                return Unauthorized();

            RoleDBO? userRole = null;

            if (user.RoleId != null)
                roleRepository.GetRoleGroup(user.RoleId!).TryGetValue(out userRole);

            // Create JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!);

            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Username!),
                new(CustomClaims.UserId, user.UID!)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(30)),
                Issuer = config["JwtSettings:Issuer"]!,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return Ok(jwt);
        }

        [HttpPost("self/changepwd")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;

            if (!userRepository.GetUser(uid: userId).TryGetValue(out var user))
                return NotFound();

            string computedHash = CryptographyUtility.ComputeSHA256Hash(dto.OldPassword!);

            if (computedHash != user.Password)
                return StatusCode(403, "Incorrect password!");

            if (!userRepository.ChangeUserPassword(userId, CryptographyUtility.ComputeSHA256Hash(dto.NewPassword)).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }

        [HttpGet("self")]
        public IActionResult GetSelfUser()
        {
            string? userId = HttpContext.GetClaim(CustomClaims.UserId);

            if (!userRepository.GetUser(uid: userId).TryGetValue(out var user))
                return NotFound();

            user.Password = null;

            return Ok(user);
        }

        [HttpGet("all")]
        //! MAX 100.000 Kullanıcı (Pagination yok!!!)
        public IActionResult GetAllUsernames()
        {
            if (!userRepository.GetFirst100000Users().TryGetValue(out var users))
                return NotFound();

            return Ok(users.Select(user =>
            {
                user.Password = null;
                return user;
            }));
        }

        [HttpGet("self/pfp")]
        public IActionResult GetUserPfp()
        {
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;

            if (!fileService.TryGetUserPfp(userId, out byte[]? buffer, out string? ext) || buffer == null || ext == null)
                return NotFound();

            return Ok(new
            {
                FileExtension = ext,
                Data = Convert.ToBase64String(buffer)
            });
        }

        [HttpPost("self/pfp")]
        public async Task<IActionResult> SetUserPfp([FromQuery, BindRequired] string fileType)
        {
            if (Request.ContentLength > Convert.ToInt64(config["qdms:MaxDocumentSizeKB"]) * 1024)
                return StatusCode(413); // Content Too Large

            if (fileType != "jpeg" && fileType != "png")
                return BadRequest("File type mismatch!");

            byte[]? file = null;
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;

            using (var ms = new MemoryStream())
            {
                await Request.Body.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                if (ms.Length == 0)
                    return BadRequest();

                file = Convert.FromBase64String(Encoding.UTF8.GetString(ms.ToArray()));
            }

            if (file == null || file.Length == 0)
                return BadRequest("Empty file");

            if (!fileService.UploadUserPfp(userId, fileType, file!))
                return StatusCode(500);

            return Ok();
        }

        [HttpDelete("{uid:length(9):required}")]
        public IActionResult DeleteUser(string uid)
        {
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;

            if (uid != userId && !HttpContext.GetPerms().HasFlag(ActionPerm.UserDelete))
                return Forbid();

            if (!userRepository.DeleteUser(uid).IsSuccessful)
                return StatusCode(500);

            agendaRepository.DeleteEvents(uid);

            return Ok();
        }

        [HttpPost, RequirePermission(ActionPerm.UserCreate)]
        public IActionResult CreateNewUser([FromBody] CreateUserDTO dto)
        {
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;
            UserDBO dbo = new UserDBO(dto, userId);

            if (!userRepository.InsertUser(dbo).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }


        [HttpPut("{uid:length(9):required}"), RequirePermission(ActionPerm.UserModify)]
        public IActionResult ChangeUser([FromBody] ChangeUserDTO dto, string uid)
        {
            if (!userRepository.UpdateUser(dto, uid).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }

        [HttpPut("self")]
        public IActionResult ChangeSelfUser([FromBody] ChangeUserDTO dto)
        {
            string userId = HttpContext.GetClaim(CustomClaims.UserId)!;

            if (!userRepository.UpdateUser(dto, userId).IsSuccessful)
                return StatusCode(500);

            return Ok();
        }

        [HttpPost("self/pwd/sendcode"), AllowAnonymous]
        public IActionResult SendResetPwdCode([FromQuery(Name = "user")] string value)
        {
            if (!userRepository.GetUser(username: value, email: value, useOr: true).TryGetValue(out var user))
                return NotFound();

            var dbo = new VerificationCodeDBO(user.UID!);
            if (!verCodeRepository.CreateCode(dbo).IsSuccessful)
                return StatusCode(500);

           emailService.AddEmailToQueue(user.Email!, new EmailTemplates.ResetPasswordEmailTemplate(dbo.Code));

            return Ok(dbo.Id);
        }

        [HttpPost("self/pwd/checkcode"), AllowAnonymous]
        public IActionResult SendResetPwdCode([FromQuery] string verificationId, [FromQuery] string code)
        {
            if (!verCodeRepository.GetCode(verificationId).TryGetValue(out var dbo) || code.ToUpper() != dbo.Code.ToUpper())
                return NotFound();

            return Ok();
        }

        [HttpPost("self/pwd/reset"), AllowAnonymous]
        public IActionResult SendResetPwdCode([FromBody] ResetEmailDTO dto)
        {
            if (!verCodeRepository.GetCode(dto.CodeId).TryGetValue(out var verCode) || verCode.Code.ToUpper() != dto.Code.ToUpper())
                return NotFound();

            verCodeRepository.DeleteCode(dto.CodeId);
            userRepository.ChangeUserPassword(verCode.Uid, CryptographyUtility.ComputeSHA256Hash(dto.NewPassword));

            return Ok();
        }
    }
    public class CustomClaims
    {
        public const string UserId = "userid";
    }
}
