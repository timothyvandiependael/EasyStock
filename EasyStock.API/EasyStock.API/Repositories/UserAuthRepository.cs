using EasyStock.API.Data;
using EasyStock.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Repositories
{
    public class UserAuthRepository : IUserAuthRepository
    {
        private readonly AuthContext _dbContext;

        public UserAuthRepository(AuthContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserAuth?> GetByUserNameAsync(string userName)
        {
            return await _dbContext.UserAuths.SingleOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<UserAuth?> GetByIdAsync(int id)
        {
            return await _dbContext.UserAuths.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAsync(UserAuth userAuth)
        {
            await _dbContext.UserAuths.AddAsync(userAuth);
            await _dbContext.SaveChangesAsync();
        }
    }
}
