using Xunit;
using Moq;
using EasyStock.API.Services;
using EasyStock.API.Repositories;
using EasyStock.API.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using EasyStock.API.Dtos;
using Microsoft.AspNetCore.Identity;

namespace EasyStock.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserAuthRepository> _userAuthRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IUserPermissionRepository> _userPermissionRepositoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userAuthRepositoryMock = new Mock<IUserAuthRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _userPermissionRepositoryMock = new Mock<IUserPermissionRepository>();

            var jwtSectionMock = new Mock<IConfigurationSection>();
            jwtSectionMock.Setup(x => x["SecretKey"]).Returns("your_test_secret_key_1234567890ABCDEF");
            jwtSectionMock.Setup(x => x["Issuer"]).Returns("TestIssuer");
            jwtSectionMock.Setup(x => x["Audience"]).Returns("TestAudience");

            _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSectionMock.Object);

            _authService = new AuthService(_userAuthRepositoryMock.Object,
                _configurationMock.Object,
                _userPermissionRepositoryMock.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var userName = "testuser";
            var password = "password123";
            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                Role = API.Common.UserRole.Regular,
                MustChangePassword = false,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, password);

            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.AuthenticateAsync(userName, password);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.False(result.MustChangePassword);

        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync((UserAuth?)null);

            // Act
            var result = await _authService.AuthenticateAsync("nonexistent", "any_password");
            
            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var userName = "testuser";
            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, "correct_password");

            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.AuthenticateAsync(userName, "wrong_password");

            // Assert
            Assert.Null(result);
        }



        [Fact]
        public async Task ChangePasswordAsync_Success_WhenMustChangePasswordTrue()
        {
            // Arrange
            var userAuth = new UserAuth
            {
                UserName = "testuser",
                PasswordHash = "",
                MustChangePassword = true,
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync("testuser")).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.ChangePasswordAsync("testuser", "oldpoassword", "newpassword");

            Assert.True(result.Success);
            _userAuthRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once());
            Assert.False(userAuth.MustChangePassword);
        }

        public async Task ChangePasswordAsync_Success_WhenOldPasswordCorrect()
        {
            // Arrange
            var userName = "testuser";

            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                MustChangePassword = false,
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, "correctoldpassword");

            _userAuthRepositoryMock.Setup(r => r.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.ChangePasswordAsync(userName, "correctoldpassword", "newpassword");

            // Assert
            Assert.True(result.Success);
            _userAuthRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            var hasher = new PasswordHasher<UserAuth>();
            var verificationResult = hasher.VerifyHashedPassword(userAuth, userAuth.PasswordHash, "newpassword");
            Assert.Equal(PasswordVerificationResult.Success, verificationResult);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsError_WhenUserNotFound()
        {
            // Arrange
            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync(It.IsAny<string>())).ReturnsAsync((UserAuth?)null);

            // Act
            var result = await _authService.ChangePasswordAsync("nonexistant", "any", "any");

            // Assert
            Assert.False(result.Success);
            Assert.True(result.Errors.ContainsKey("userId"));
            Assert.Contains("User not found", result.Errors["userId"]);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsError_WhenOldPasswordNotProvided()
        {
            // Arrange
            var userName = "testuser";

            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                MustChangePassword = false,
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, "correctoldpassword");

            _userAuthRepositoryMock.Setup(repo => repo.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.ChangePasswordAsync(userName, null, "newpassword");

            // Assert
            Assert.False(result.Success);
            Assert.True(result.Errors.ContainsKey("oldPassword"));
            Assert.Contains("no old password provided", result.Errors["oldPassword"]);

        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsError_WhenOldPasswordWrong()
        {
            // Arrange
            var userName = "testuser";

            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                MustChangePassword = false,
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, "correctoldpassword");

            _userAuthRepositoryMock.Setup(r => r.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.ChangePasswordAsync(userName, "wrongoldpassword", "newpassword");

            // Assert
            Assert.False(result.Success);
            Assert.True(result.Errors.ContainsKey("oldPassword"));
            Assert.Contains("The given old password was incorrect", result.Errors["oldPassword"]);
        }



        [Fact]

        public async Task AddAsync_CreatesUserAuthWithHashedPassword_AndReturnsPassword()
        {
            // Arrange
            var user = new User
            {
                UserName = "newuser",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            var role = EasyStock.API.Common.UserRole.Regular;
            var creationUserName = "admin";

            UserAuth? capturedUserAuth = null;
            _userAuthRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<UserAuth>()))
                .Callback<UserAuth>(ua => capturedUserAuth = ua)
                .Returns(Task.CompletedTask);

            // Act
            var generatedPassword = await _authService.AddAsync(user, role, creationUserName);

            // Assert
            Assert.NotNull(capturedUserAuth);
            Assert.Equal(user.UserName, capturedUserAuth!.UserName);
            Assert.Equal(role, capturedUserAuth.Role);
            Assert.Equal(creationUserName, capturedUserAuth.CrUserId);
            Assert.Equal(creationUserName, capturedUserAuth.LcUserId);
            Assert.False(string.IsNullOrEmpty(capturedUserAuth.PasswordHash));
            Assert.NotEqual(string.Empty, capturedUserAuth.PasswordHash);
            Assert.False(string.IsNullOrEmpty(generatedPassword));
            Assert.InRange(generatedPassword.Length, 8, 40); 
            _userAuthRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserAuth>()), Times.Once);
        }



        [Fact]
        public async Task UserExists_ReturnsTrue_WhenUserIsFound()
        {
            // Arrange
            var userName = "testuser";

            var userAuth = new UserAuth
            {
                UserName = userName,
                PasswordHash = "",
                MustChangePassword = false,
                Role = API.Common.UserRole.Regular,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "test",
                LcUserId = "test"
            };

            userAuth.PasswordHash = _authService.HashPassword(userAuth, "anypassword");

            _userAuthRepositoryMock.Setup(r => r.GetByUserNameAsync(userName)).ReturnsAsync(userAuth);

            // Act
            var result = await _authService.UserExists(userName);

            // Assert
            Assert.True(result);
            _userAuthRepositoryMock.Verify(r => r.GetByUserNameAsync(userName), Times.Once);
        }

        [Fact]
        public async Task UserExists_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var userName = "invaliduser";

            _userAuthRepositoryMock.Setup(r => r.GetByUserNameAsync(userName)).ReturnsAsync((UserAuth?)null);

            // Act
            var result = await _authService.UserExists(userName);

            // Assert
            Assert.False(result);
            _userAuthRepositoryMock.Verify(r => r.GetByUserNameAsync(userName), Times.Once);
        }
    }
}
