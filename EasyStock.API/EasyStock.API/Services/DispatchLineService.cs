using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class DispatchLineService : IDispatchLineService
    {
        private readonly IDispatchLineRepository _dispatchLineRepository;

        public DispatchLineService(IDispatchLineRepository dispatchLineRepository)
        {
            _dispatchLineRepository = dispatchLineRepository;
        }

        public async Task<IEnumerable<DispatchLineOverview>> GetAllAsync()
            => await _dispatchLineRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _dispatchLineRepository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
