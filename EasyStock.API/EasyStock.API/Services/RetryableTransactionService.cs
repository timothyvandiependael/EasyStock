using EasyStock.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Services
{
    public class RetryableTransactionService : IRetryableTransactionService
    {
        private readonly AppDbContext _context;

        public RetryableTransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            var attempt = 1;
            while (attempt <= 3)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    if (++attempt > 3)
                        throw;
                    await Task.Delay(100 * attempt);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
