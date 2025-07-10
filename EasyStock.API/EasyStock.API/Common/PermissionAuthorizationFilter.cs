using EasyStock.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace EasyStock.API.Common
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationFilter(string resource, string action, IPermissionService permissionService)
        {
            _resource = resource;
            _action = action;
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? false)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userName = user.Identity!.Name!;
            var hasPermission = await _permissionService.HasPermissionAsync(userName, _resource, _action);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }


        }
    }
}
