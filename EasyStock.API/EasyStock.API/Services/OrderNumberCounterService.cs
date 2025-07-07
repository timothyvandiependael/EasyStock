using EasyStock.API.Common;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class OrderNumberCounterService : IOrderNumberCounterService
    {
        private readonly IOrderNumberCounterRepository _repository;

        public OrderNumberCounterService(IOrderNumberCounterRepository repository)
        { 
            _repository = repository;
        }

        public async Task<string> GenerateOrderNumberAsync(OrderType orderType)
        {
            var prefix = orderType switch
            {
                OrderType.PurchaseOrder => "PO",
                OrderType.SalesOrder => "SO",
                OrderType.Reception => "RE",
                OrderType.Dispatch => "DI",
                _ => throw new Exception("Ordertype not supported")
            };
            var today = DateTime.UtcNow;
            var lastNumber = await _repository.GetNextOrderNumberAsync(orderType, DateOnly.FromDateTime(today));

            return $"{prefix}-{today:yyyyMMdd}-{lastNumber:D5}";
        }
    }
}
