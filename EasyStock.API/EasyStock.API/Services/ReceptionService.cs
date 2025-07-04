using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ReceptionService : IReceptionService
    {
        private readonly IReceptionRepository _receptionRepository;

        public ReceptionService(IReceptionRepository receptionRepository)
        {
            _receptionRepository = receptionRepository;
        }

        public async Task<IEnumerable<ReceptionOverview>> GetAllAsync()
            => await _receptionRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionRepository.GetAdvancedAsync(filters, sorting, pagination);

    }
}
