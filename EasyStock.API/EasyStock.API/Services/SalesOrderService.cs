using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _SalesOrderRepository;

        public SalesOrderService(ISalesOrderRepository SalesOrderRepository)
        {
            _SalesOrderRepository = SalesOrderRepository;
        }

        public async Task<IEnumerable<SalesOrderOverview>> GetAllAsync()
            => await _SalesOrderRepository.GetAllAsync();

        public async Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _SalesOrderRepository.GetAdvancedAsync(filters, sorting, pagination);

    }
}
