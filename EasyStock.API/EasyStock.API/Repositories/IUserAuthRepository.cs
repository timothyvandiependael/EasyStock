using EasyStock.API.Models;
namespace EasyStock.API.Repositories
{
    public interface IUserAuthRepository
    {
        Task<UserAuth?> GetByUserNameAsync(string userName);
        Task<UserAuth?> GetByIdAsync(int id);
        Task SaveChangesAsync();
        Task AddAsync(UserAuth userAuth);
    }
}
