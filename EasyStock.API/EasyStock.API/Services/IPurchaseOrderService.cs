using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync();
        Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task AddAsync(PurchaseOrder entity, string userName, bool useTransaction = true);
        Task DeleteAsync(int id, string userName, bool useTransaction = true);
        Task BlockAsync(int id, string userName, bool useTransaction = true);
        Task UnblockAsync(int id, string userName, bool useTransaction = true);
        Task<List<PurchaseOrder>> AddFromSalesOrder(int salesOrderId, Dictionary<int, int> productSuppliers, string userName, bool useTransaction = true);
        Task<PurchaseOrder?> AutoRestockProduct(int productId, string userName, bool useTransaction = true);
        Task<int> GetNextLineNumberAsync(int id);
    }
}
