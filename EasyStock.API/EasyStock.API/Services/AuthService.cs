using EasyStock.API.Repositories;
using EasyStock.API.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;

namespace EasyStock.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserAuthRepository _userAuthRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<UserAuth> _passwordHasher;

        public AuthService(IUserAuthRepository userAuthRepository, IConfiguration configuration)
        {
            _userAuthRepository = userAuthRepository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<UserAuth>();
        }

        public async Task<string?> AuthenticateAsync(string userName, string password)
        {
            var authUser = await _userAuthRepository.GetByUserNameAsync(userName);
            if (authUser == null)
                return null;

            var verificationResult = _passwordHasher.VerifyHashedPassword(authUser, authUser.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
                return null;

            return GenerateJwtToken(authUser);
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

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userAuth.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, userAuth.Role)
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
    }
}
