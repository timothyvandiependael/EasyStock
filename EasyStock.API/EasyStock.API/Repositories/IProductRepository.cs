using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductOverview>> GetAllAsync();
        Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination? pagination);

        Task AddSupplierAsync(int id, int supplierId);

        Task RemoveSupplierAsync(int id, int supplierId);
    }
}
