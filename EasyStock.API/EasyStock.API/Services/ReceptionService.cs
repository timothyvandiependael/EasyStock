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
        private readonly IReceptionLineProcessor _receptionLineProcessor;

        public ReceptionService(IReceptionRepository receptionRepository, IOrderNumberCounterService orderNumberCounterService, IRepository<Reception> repository, IRetryableTransactionService retryableTransactionService, IReceptionLineProcessor receptionLineProcessor)
        {
            _receptionRepository = receptionRepository;
            _orderNumberCounterService = orderNumberCounterService;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _receptionLineProcessor = receptionLineProcessor;
        }

        public async Task<IEnumerable<ReceptionOverview>> GetAllAsync()
            => await _receptionRepository.GetAllAsync();

        public async Task<PaginationResult<ReceptionOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _receptionRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(Reception entity, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await AddAsyncInternal(entity, userName);
                });
            }
            else
            {
                await AddAsyncInternal(entity, userName);
            }
                
        }

        private async Task AddAsyncInternal(Reception entity, string userName)
        {
            entity.ReceptionNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.Reception);
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;

            var lines = entity.Lines.ToList();
            entity.Lines.Clear();

            var lineCounter = 1;
            foreach (var line in lines)
            {
                line.LineNumber = lineCounter;
                await _receptionLineProcessor.AddAsync(line, userName, null, true);

                lineCounter++;
            }

            await _repository.AddAsync(entity);
        }

        public async Task DeleteAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await DeleteAsyncInternal(id, userName);
                });
            }
            else
            {
                await DeleteAsyncInternal(id, userName);
            }
                
        }

        private async Task DeleteAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to delete record with ID {id}");

            foreach (var line in entity.Lines)
            {
                await _receptionLineProcessor.DeleteAsync(line.Id, userName);
            }
            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await BlockAsyncInternal(id, userName);
                });
            }
            else
            {
                await BlockAsyncInternal(id, userName);
            }
                
        }

        private async Task BlockAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            entity.BlDate = DateTime.UtcNow;
            entity.BlUserId = userName;

            foreach (var line in entity.Lines)
            {
                await _receptionLineProcessor.BlockAsync(line.Id, userName);
            }
            await _repository.UpdateAsync(entity);
        }

        public async Task UnblockAsync(int id, string userName, bool useTransaction = true)
        {
            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    await UnblockAsyncInternal(id, userName);
                });
            }
            else
            {
                await UnblockAsyncInternal(id, userName);
            }
        }

        private async Task UnblockAsyncInternal(int id, string userName)
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
                await _receptionLineProcessor.UnblockAsync(line.Id, userName);
            }
            await _repository.UpdateAsync(entity);
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
