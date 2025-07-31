using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface ISupplierService
    {
        Task<PaginationResult<Supplier>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);
        Task AddProductAsync(int id, int productId);
        Task RemoveProductAsync(int id, int productId);
    }
}
