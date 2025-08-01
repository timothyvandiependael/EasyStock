using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IUserService
    {
        Task AddAsync(User entity, string userName);
        Task UpdateAsync(User entity, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
