using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QDMS.Controllers;
using QDMS.DBOs;
using QDMS.Repositories;

namespace QDMS.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly ActionPerm[] _perms;

        public RequirePermissionAttribute(params ActionPerm[] perms)
        {
            _perms = perms;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string? userId = context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == CustomClaims.UserId)?.Value;

            var userRepository = context.HttpContext.RequestServices.GetService<UserRepository>();
            var roleRepository = context.HttpContext.RequestServices.GetService<RoleRepository>();


            if (!string.IsNullOrEmpty(userId) && userRepository != null && roleRepository != null)
            {
                var userResult = userRepository.GetUser(uid: userId);

                if (userResult.IsSuccessful && !string.IsNullOrEmpty(userResult.Value.RoleId))
                {
                    var roleResult = roleRepository.GetRoleGroup(userResult.Value.RoleId);

                    if (roleResult.IsSuccessful && CheckForPerms(_perms, roleResult.Value.Permissions))
                        return;
                }
            }

            context.Result = new ObjectResult("Insufficient permission")
            {
                StatusCode = 403
            };
        }

        private bool CheckForPerms(ActionPerm[] allowedPerms, ActionPerm userPerms)
        {
            foreach (ActionPerm perm in allowedPerms)
            {
                if (userPerms.HasFlag(perm))
                    return true;
            }
            
            return false;
        }
    }
}
