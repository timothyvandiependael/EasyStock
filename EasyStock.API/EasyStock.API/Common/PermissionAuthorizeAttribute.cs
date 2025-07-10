using Microsoft.AspNetCore.Mvc;

namespace EasyStock.API.Common
{
    public class PermissionAuthorizeAttribute : TypeFilterAttribute
    {
        public PermissionAuthorizeAttribute(string resource, string action) : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { resource, action };
        }
    }
}
