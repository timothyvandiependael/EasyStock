using EasyStock.API.Models;
namespace EasyStock.API.Repositories
{
    public interface IUserAuthRepository
    {
        Task<UserAuth?> GetByUserNameAsync(string userName);
    }
}
