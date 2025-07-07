using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public interface IOrderNumberCounterService
    {
        Task<string> GenerateOrderNumberAsync(OrderType orderType);
    }
}
