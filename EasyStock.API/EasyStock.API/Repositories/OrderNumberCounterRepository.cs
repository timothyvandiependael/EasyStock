using EasyStock.API.Common;
using EasyStock.API.Data;
using Microsoft.EntityFrameworkCore;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public class OrderNumberCounterRepository : IOrderNumberCounterRepository
    {
        private readonly AppDbContext _context;

        public OrderNumberCounterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetNextOrderNumberAsync(OrderType orderType, DateOnly date)
        {
            var counter = await _context.OrderNumberCounters.FirstOrDefaultAsync(c => c.OrderType == orderType && c.Date == date);

            if (counter == null)
            {
                counter = new OrderNumberCounter
                {
                    OrderType = orderType,
                    Date = date,
                    LastNumber = 1
                };

                _context.OrderNumberCounters.Add(counter);
            }
            else
            {
                counter.LastNumber++;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var x = ex;
                throw;
            }

            return counter.LastNumber;
        }
    }
}
