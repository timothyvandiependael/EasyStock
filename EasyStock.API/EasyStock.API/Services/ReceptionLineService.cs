using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ReceptionLineService : IReceptionLineService
    {
        private readonly IReceptionLineRepository _receptionLineRepository;

        public ReceptionLineService(IReceptionLineRepository receptionLineRepository)
        {
            _receptionLineRepository = receptionLineRepository;
        }

        public async Task<IEnumerable<ReceptionLineOverview>> GetAllAsync()
            => await _receptionLineRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionLineOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionLineRepository.GetAdvancedAsync(filters, sorting, pagination);
    }
}
