using EasyStock.API.Models;
using EasyStock.API.Dtos;
using EasyStock.API.Dtos.Auth;

namespace EasyStock.API.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto?> AuthenticateAsync(string userName, string password);
        string HashPassword(UserAuth userAuth, string password);
        Task<ChangePasswordResultDto> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<string> AddAsync(User user, string role, string creationUserName);
        Task<bool> UserExists(string userName);
    }
}
