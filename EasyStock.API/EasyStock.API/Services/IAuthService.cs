using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string userName, string password);

        string HashPassword(UserAuth userAuth, string password);
    }
}
