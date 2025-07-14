using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IPurchaseOrderLineProcessor
    {
        Task AddAsync(PurchaseOrderLine entity, string userName, Func<int, Task<int>>? getNextLineNumberAsync, 
            bool fromParent = false);
        Task DeleteAsync(int id, string userName);
        Task BlockAsync(int id, string userName);
        Task UnblockAsync(int id, string userName);
    }
}
