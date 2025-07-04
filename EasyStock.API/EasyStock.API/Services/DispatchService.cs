using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _dispatchRepository;

        public DispatchService(IDispatchRepository dispatchRepository)
        {
            _dispatchRepository = dispatchRepository;
        }

        public async Task<IEnumerable<DispatchOverview>> GetAllAsync()
            => await _dispatchRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _dispatchRepository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
