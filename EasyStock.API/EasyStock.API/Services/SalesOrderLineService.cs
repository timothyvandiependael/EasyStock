using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class SalesOrderLineService : ISalesOrderLineService
    {
        private readonly ISalesOrderLineRepository _SalesOrderLineRepository;

        public SalesOrderLineService(ISalesOrderLineRepository SalesOrderLineRepository)
        {
            _SalesOrderLineRepository = SalesOrderLineRepository;
        }

        public async Task<IEnumerable<SalesOrderLineOverview>> GetAllAsync()
            => await _SalesOrderLineRepository.GetAllAsync();

        public async Task<PaginationResult<SalesOrderLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _SalesOrderLineRepository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
