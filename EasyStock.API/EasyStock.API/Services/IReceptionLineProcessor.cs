using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IReceptionLineProcessor
    {
        Task SetPOStatusFields(int quantity, int purchaseOrderLineId, string userName);
        Task AddAsync(ReceptionLine entity, string userName, Func<int, Task<int>>? getNextLineNumberAsync,
            bool fromParent = false);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
