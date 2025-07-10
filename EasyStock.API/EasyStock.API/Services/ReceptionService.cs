using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class ReceptionService : IReceptionService
    {
        private readonly IReceptionRepository _receptionRepository;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IRepository<Reception> _repository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IReceptionLineService _receptionLineService;

        public ReceptionService(IReceptionRepository receptionRepository, IOrderNumberCounterService orderNumberCounterService, IRepository<Reception> repository, IRetryableTransactionService retryableTransactionService, IReceptionLineService receptionLineService)
        {
            _receptionRepository = receptionRepository;
            _orderNumberCounterService = orderNumberCounterService;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _receptionLineService = receptionLineService;
        }

        public async Task<IEnumerable<ReceptionOverview>> GetAllAsync()
            => await _receptionRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(Reception entity, string userName)
        {
            entity.ReceptionNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.Reception);
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;

            var lineCounter = 1;
            foreach (var line in entity.Lines)
            {
                line.CrDate = DateTime.UtcNow;
                line.LcDate = line.CrDate;
                line.CrUserId = userName;
                line.LcUserId = userName;
                line.LineNumber = lineCounter;

                lineCounter++;
            }

            await _repository.AddAsync(entity);
        }

        public async Task DeleteAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    throw new InvalidOperationException($"Unable to delete record with ID {id}");

                foreach (var line in entity.Lines)
                {
                    await _receptionLineService.DeleteAsync(line.Id, userName, false);
                }
                await _repository.DeleteAsync(id);
            });
        }

        public async Task BlockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    throw new InvalidOperationException($"Unable to block record with ID {id}");
                entity.BlDate = DateTime.UtcNow;
                entity.BlUserId = userName;

                foreach (var line in entity.Lines)
                {
                    await _receptionLineService.BlockAsync(line.Id, userName, false);
                }
                await _repository.UpdateAsync(entity);
            });
        }

        public async Task UnblockAsync(int id, string userName)
        {
            await _retryableTransactionService.ExecuteAsync(async () =>
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    throw new InvalidOperationException($"Unable to unblock record with ID {id}");
                entity.BlDate = null;
                entity.BlUserId = null;
                entity.LcDate = DateTime.UtcNow;
                entity.LcUserId = userName;

                foreach (var line in entity.Lines)
                {
                    await _receptionLineService.UnblockAsync(line.Id, userName, false);
                }
                await _repository.UpdateAsync(entity);
            });
        }

        public async Task<int> GetNextLineNumberAsync(int id)
        {
            var re = await _repository.GetByIdAsync(id);
            if (re == null) throw new Exception($"Reception with id {id} not found.");
            var nextLineNumber = re.Lines.Any() ? re.Lines.Max(l => l.LineNumber) + 1 : 1;
            return nextLineNumber;
        }

    }
}
