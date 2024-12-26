using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QDMS.Classes;
using QDMS.CustomAttributes;
using QDMS.DBOs;
using QDMS.DTOs;
using QDMS.Repositories;

namespace QDMS.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    public class RoleController(RoleRepository roleRepository) : Controller
    {
        [HttpGet("{id:required:length(9)}")]
        [AllowAnonymous]
        public IActionResult GetRole([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var roleResult = roleRepository.GetRoleGroup(id);

            if (roleResult.IsError)
                return StatusCode(500);

            if (!roleResult.IsSuccessful)
                return NotFound();

            RoleDBO role = roleResult.Value;

            return Ok(role);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetRoles()
        {
            var roleResult = roleRepository.GetRoleGroups();

            if (roleResult.IsError)
                return StatusCode(500);

            if (!roleResult.IsSuccessful)
                return NotFound();

            return Ok(roleResult.Value);
        }

        [HttpPut("{id:required:length(9)}")]
        [RequirePermission(ActionPerm.RoleManage)]
        public IActionResult UpdateRole([FromRoute] string id, [FromBody] UpdateRoleDTO dto)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(dto.Name))
                return BadRequest();

            var roleResult = roleRepository.UpdateRoleGroup(id, dto.Name!, dto.Permissions);

            if (roleResult.IsError)
                return StatusCode(500);

            if (!roleResult.IsSuccessful)
                return NotFound();
            
            return Ok(new RoleDBO(id).Fill(dto));
        }

        [HttpDelete("{id:required:length(9)}")]
        [RequirePermission(ActionPerm.RoleManage)]
        public IActionResult DeleteRole([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var roleResult = roleRepository.DeleteRoleGroup(id);

            if (roleResult.IsError)
                return StatusCode(500);

            if (!roleResult.IsSuccessful)
                return NotFound();

            return Ok();
        }


        [HttpPost]
        [RequirePermission(ActionPerm.RoleManage)]
        public IActionResult CreateRole([FromBody] UpdateRoleDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Name))
                return BadRequest();

            var newRoleDBO = new RoleDBO(CryptographyUtility.GenerateId(9)).Fill(dto);
            var roleResult = roleRepository.CreateRoleGroup(newRoleDBO);

            if (roleResult.IsError)
                return StatusCode(500);

            if (!roleResult.IsSuccessful)
                return BadRequest();

            return Ok(newRoleDBO);
        }
    }
}
