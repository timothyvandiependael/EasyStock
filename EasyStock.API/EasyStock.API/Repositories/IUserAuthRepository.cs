using EasyStock.API.Common;
using EasyStock.API.Models;
namespace EasyStock.API.Repositories
{
    public interface IUserAuthRepository
    {
        Task<UserAuth?> GetByUserNameAsync(string userName);
        Task<UserAuth?> GetByIdAsync(int id);
        Task SaveChangesAsync();
        Task AddAsync(UserAuth userAuth);
        Task UpdateAsync(UserAuth userAuth);
        Task<Dictionary<string, UserRole>> GetRolesAsync();
    }
}
