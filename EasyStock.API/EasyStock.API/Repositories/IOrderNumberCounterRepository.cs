using EasyStock.API.Common;

namespace EasyStock.API.Repositories
{
    public interface IOrderNumberCounterRepository
    {
        Task<int> GetNextOrderNumberAsync(OrderType orderType, DateOnly date);
    }
}
