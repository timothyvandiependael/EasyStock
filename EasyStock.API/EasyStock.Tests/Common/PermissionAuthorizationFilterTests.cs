using System.Security.Claims;
using System.Threading.Tasks;
using EasyStock.API.Common;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace EasyStock.Tests.Common
{
    public class PermissionAuthorizationFilterTests
    {
        private static AuthorizationFilterContext CreateFilterContext(ClaimsPrincipal user)
        {
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ControllerActionDescriptor(),
                new ModelStateDictionary());

            return new AuthorizationFilterContext(actionContext, new System.Collections.Generic.List<IFilterMetadata>());
        }

        [Fact]
        public async Task OnAuthorizationAsync_UserNotAuthenticated_SetsUnauthorizedResult()
        {
            // Arrange
            var permissionService = new Mock<IPermissionService>();
            var filter = new PermissionAuthorizationFilter("resource", "action", permissionService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated
            var context = CreateFilterContext(user);

            // Act
            await filter.OnAuthorizationAsync(context);

            // Assert
            Assert.IsType<UnauthorizedResult>(context.Result);
        }

        [Fact]
        public async Task OnAuthorizationAsync_UserAuthenticatedWithoutPermission_SetsForbidResult()
        {
            // Arrange
            var permissionService = new Mock<IPermissionService>();
            permissionService
                .Setup(s => s.HasPermissionAsync("testuser", "resource", "action"))
                .ReturnsAsync(false);

            var filter = new PermissionAuthorizationFilter("resource", "action", permissionService.Object);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var context = CreateFilterContext(user);

            // Act
            await filter.OnAuthorizationAsync(context);

            // Assert
            Assert.IsType<ForbidResult>(context.Result);
        }

        [Fact]
        public async Task OnAuthorizationAsync_UserAuthenticatedWithPermission_DoesNotSetResult()
        {
            // Arrange
            var permissionService = new Mock<IPermissionService>();
            permissionService
                .Setup(s => s.HasPermissionAsync("testuser", "resource", "action"))
                .ReturnsAsync(true);

            var filter = new PermissionAuthorizationFilter("resource", "action", permissionService.Object);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var context = CreateFilterContext(user);

            // Act
            await filter.OnAuthorizationAsync(context);

            // Assert
            Assert.Null(context.Result);
        }
    }
}
