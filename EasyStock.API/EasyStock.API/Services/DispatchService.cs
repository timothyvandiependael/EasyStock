using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _dispatchRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<Dispatch> _repository;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IDispatchLineService _dispatchLineService;


        public DispatchService(IDispatchRepository dispatchRepository, IDispatchLineService dispatchLineService, IRetryableTransactionService retryableTransactionService, IRepository<Dispatch> repository, IOrderNumberCounterService orderNumberCounterService)
        {
            _dispatchRepository = dispatchRepository;
            _dispatchLineService = dispatchLineService;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _orderNumberCounterService = orderNumberCounterService;

        }

        public async Task<IEnumerable<DispatchOverview>> GetAllAsync()
            => await _dispatchRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _dispatchRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(Dispatch entity, string userName)
        {
            entity.DispatchNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.Dispatch);
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;

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
                    await _dispatchLineService.DeleteAsync(line.Id, userName, false);
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
                    await _dispatchLineService.BlockAsync(line.Id, userName, false);
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
                    await _dispatchLineService.UnblockAsync(line.Id, userName, false);
                }
                await _repository.UpdateAsync(entity);
            });
        }

        public async Task<int> GetNextLineNumberAsync(int id)
        {
            var di = await _repository.GetByIdAsync(id);
            if (di == null) throw new Exception($"Dispatch with id {id} not found.");
            var nextLineNumber = di.Lines.Any() ? di.Lines.Max(l => l.LineNumber) + 1 : 1;
            return nextLineNumber;
        }
    }
}
