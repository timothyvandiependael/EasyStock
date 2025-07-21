using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EasyStock.API.Services;
using EasyStock.API.Repositories;
using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class PermissionServiceTests
    {
        private readonly Mock<IUserAuthRepository> _authRepoMock;
        private readonly Mock<IUserPermissionRepository> _permRepoMock;
        private readonly PermissionService _service;

        private readonly EntityFactory _entityFactory; // ✅ private readonly

        public PermissionServiceTests()
        {
            _authRepoMock = new Mock<IUserAuthRepository>();
            _permRepoMock = new Mock<IUserPermissionRepository>();
            _service = new PermissionService(_authRepoMock.Object, _permRepoMock.Object);

            _entityFactory = new EntityFactory(); // ✅ set in constructor
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsFalse_WhenUserNotFound()
        {
            _authRepoMock.Setup(r => r.GetByUserNameAsync("missingUser"))
                         .ReturnsAsync((UserAuth)null);

            var result = await _service.HasPermissionAsync("missingUser", "ResourceA", "view");

            Assert.False(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenUserIsAdmin()
        {
            var admin = _entityFactory.CreateUserAuth();
            admin.Role = UserRole.Admin;

            _authRepoMock.Setup(r => r.GetByUserNameAsync("adminUser"))
                         .ReturnsAsync(admin);

            var result = await _service.HasPermissionAsync("adminUser", "Anything", "edit");

            Assert.True(result);
            _permRepoMock.Verify(r => r.GetPermissionsForUser(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsFalse_WhenPermissionsAreEmpty()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(new List<UserPermission>());

            var result = await _service.HasPermissionAsync("testUser", "ResourceA", "view");

            Assert.False(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsFalse_WhenNoMatchingPermission()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "OtherResource";
            perm.CanView = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "ResourceA", "view");

            Assert.False(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenCanViewMatches()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "ResourceA";
            perm.CanView = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "resourcea", "VIEW"); // case-insensitive

            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenCanAddMatches()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "ResourceB";
            perm.CanAdd = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "ResourceB", "add");

            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenCanEditMatches()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "ResourceC";
            perm.CanEdit = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "ResourceC", "edit");

            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsTrue_WhenCanDeleteMatches()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);


            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "ResourceD";
            perm.CanDelete = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "ResourceD", "delete");

            Assert.True(result);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsFalse_WhenActionNotRecognized()
        {
            var user = _entityFactory.CreateUserAuth();
            user.Role = UserRole.Regular;
            _authRepoMock.Setup(r => r.GetByUserNameAsync("testUser"))
                         .ReturnsAsync(user);

            var perm = _entityFactory.CreateUserPermission();
            perm.Resource = "ResourceX";
            perm.CanView = true;

            var permissions = new List<UserPermission>
        {
            perm
        };
            _permRepoMock.Setup(r => r.GetPermissionsForUser("testUser"))
                         .ReturnsAsync(permissions);

            var result = await _service.HasPermissionAsync("testUser", "ResourceX", "unknownAction");

            Assert.False(result);
        }
    }
}
