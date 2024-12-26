using Microsoft.AspNetCore.Mvc;
using QDMS.Controllers;
using QDMS.DBOs;
using QDMS.Repositories;

namespace QDMS.Classes
{
    public static class HttpContextExtensionMethods
    {
        public static string? GetClaim(this HttpContext context, string claimName)
        {
            return context.User.Claims.FirstOrDefault(x => x.Type == claimName)?.Value;
        }
        public static ActionPerm GetPerms(this HttpContext context)
        {
            string? userId = context.User.Claims.FirstOrDefault(x => x.Type == CustomClaims.UserId)?.Value;

            var userRepository = context.RequestServices.GetService<UserRepository>();
            var roleRepository = context.RequestServices.GetService<RoleRepository>();

            if (string.IsNullOrEmpty(userId) || userRepository == null || roleRepository == null)
                return ActionPerm.None;

            var userResult = userRepository.GetUser(uid: userId);

            if (!userResult.IsSuccessful || string.IsNullOrEmpty(userResult.Value.RoleId))
                return ActionPerm.None;

            var roleResult = roleRepository.GetRoleGroup(userResult.Value.RoleId);

            if (roleResult.IsSuccessful)
                return roleResult.Value.Permissions;

            return ActionPerm.None;
        }
    }
}
