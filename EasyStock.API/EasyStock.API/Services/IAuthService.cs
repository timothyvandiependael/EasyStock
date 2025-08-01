using EasyStock.API.Models;
using EasyStock.API.Dtos;
using EasyStock.API.Dtos.Auth;
using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto?> AuthenticateAsync(string userName, string password);
        string HashPassword(UserAuth userAuth, string password);
        Task<ChangePasswordResultDto> ChangePasswordAsync(string userName, string? oldPassword, string newPassword);
        Task<string> AddAsync(User user, UserRole role, string creationUserName);
        Task<bool> UserExists(string userName);
        Task UpdateRoleAsync(User user, UserRole role, string userName);

        Task<Dictionary<string, UserRole>> GetRolesAsync();
        Task<UserRole> GetRoleAsync(string userName);
    }
}
