using EasyStock.API.Common;
using EasyStock.API.Models;

namespace EasyStock.API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductOverview>> GetAllAsync();
        Task<PaginationResult<ProductOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination);

        Task<bool> IsProductBelowMinimumStock(int id);

        Task UpdateAsync(Product product, string userName, bool useTransaction = true);
    }
}
