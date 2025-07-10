using EasyStock.API.Repositories;
using EasyStock.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using EasyStock.API.Dtos;
using System.Security.Cryptography;
using EasyStock.API.Dtos.Auth;
using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAuthRepository _userAuthRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<UserAuth> _passwordHasher;
        private readonly IUserPermissionRepository _userPermissionRepository;

        public AuthService(IUserAuthRepository userAuthRepository, IConfiguration configuration, IUserPermissionRepository userPermissionRepository)
        {
            _userAuthRepository = userAuthRepository;
            _configuration = configuration;
            _userPermissionRepository = userPermissionRepository;
            _passwordHasher = new PasswordHasher<UserAuth>();
        }

        public async Task<AuthResultDto?> AuthenticateAsync(string userName, string password)
        {
            var authUser = await _userAuthRepository.GetByUserNameAsync(userName);
            if (authUser == null)
                return null;

            var verificationResult = _passwordHasher.VerifyHashedPassword(authUser, authUser.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
                return null;

            var authResult = new AuthResultDto
            {
                Token = GenerateJwtToken(authUser),
                MustChangePassword = authUser.MustChangePassword
            };

            if (authUser.MustChangePassword)
            {
                authUser.MustChangePassword = false;
                await _userAuthRepository.SaveChangesAsync();
            }

            return authResult;
        }

        public string HashPassword(UserAuth userAuth, string password)
        {
            return _passwordHasher.HashPassword(userAuth, password);
        }


        private string GenerateJwtToken(UserAuth userAuth)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, userAuth.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, userAuth.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ChangePasswordResultDto> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var result = new ChangePasswordResultDto();

            var user = await _userAuthRepository.GetByIdAsync(userId);
            if (user == null)
            {
                result.Errors.Add("userId", "User not found");
                return result;
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);
            if (verification == PasswordVerificationResult.Failed)
            {
                result.Errors.Add("oldPassword", "The given old password was incorrect.");
                return result;
            }

            user.PasswordHash = HashPassword(user, newPassword);
            await _userAuthRepository.SaveChangesAsync();

            result.Success = true;
            return result;
        }

        public async Task<string> AddAsync(User user, UserRole role, string creationUserName)
        {
            var pw = GenerateTempPassword();

            var userAuth = new UserAuth
            {
                UserName = user.UserName,
                PasswordHash = string.Empty,
                Role = role,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = creationUserName,
                LcUserId = creationUserName
            };

            userAuth.PasswordHash = HashPassword(userAuth, pw);

            await _userAuthRepository.AddAsync(userAuth);

            return pw;
        }

        private string GenerateTempPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$?_-";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            var password = new char[length];
            for (var i = 0; i < length; i++)
            {
                password[i] = chars[bytes[i] % chars.Length];
            }

            return new string(password);
        }

        public async Task<bool> UserExists(string userName)
        {
            var user = await _userAuthRepository.GetByUserNameAsync(userName);
            if (user == null) return false;
            return true;
        }
    }
}
