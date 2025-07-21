using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.AspNetCore.Components;

namespace EasyStock.API.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IDispatchRepository _dispatchRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IRepository<Dispatch> _repository;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IRepository<SalesOrder> _genericSalesOrderRepository;
        private readonly IDispatchLineProcessor _dispatchLineProcessor;

        public DispatchService(IDispatchRepository dispatchRepository, IRetryableTransactionService retryableTransactionService, IRepository<Dispatch> repository, IOrderNumberCounterService orderNumberCounterService, IRepository<SalesOrder> genericSalesOrderRepository, IDispatchLineProcessor dispatchLineProcessor)
        {
            _dispatchRepository = dispatchRepository;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _orderNumberCounterService = orderNumberCounterService;
            _genericSalesOrderRepository = genericSalesOrderRepository;
            _dispatchLineProcessor = dispatchLineProcessor;
        }

        public async Task<IEnumerable<DispatchOverview>> GetAllAsync()
            => await _dispatchRepository.GetAllAsync();

        public async Task<PaginationResult<DispatchOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _dispatchRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(Dispatch entity, string userName, bool useTransaction = true)
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

        private async Task AddAsyncInternal(Dispatch entity, string userName)
        {
            entity.DispatchNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.Dispatch);
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

                await _dispatchLineProcessor.AddAsync(line, userName, null, true);

                lineCounter++;
            }

            await _repository.AddAsync(entity);

        }

        public async Task<Dispatch?> AddFromSalesOrder(int salesOrderId, string userName, bool useTransaction = true)
        {
            Dispatch? dispatch = null;

            if (useTransaction)
            {
                await _retryableTransactionService.ExecuteAsync(async () =>
                {
                    dispatch = await AddFromSalesOrderInternal(salesOrderId, userName);

                });
            }
            else
            {
                dispatch = await AddFromSalesOrderInternal(salesOrderId, userName);
            }

            return dispatch;
        }

        private async Task<Dispatch?> AddFromSalesOrderInternal(int salesOrderId, string userName)
        {
            Dispatch? dispatch;

            var so = await _genericSalesOrderRepository.GetByIdAsync(salesOrderId);
            if (so == null) throw new Exception($"Sales order with id {salesOrderId} not found.");

            dispatch = new Dispatch
            {
                DispatchNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.Dispatch),
                ClientId = so.ClientId,
                Client = so.Client,
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                Lines = new List<DispatchLine>()
            };

            foreach (var line in so.Lines)
            {
                var dispatchLine = new DispatchLine
                {
                    LineNumber = line.LineNumber,
                    Dispatch = dispatch,
                    ProductId = line.ProductId,
                    Product = line.Product,
                    Quantity = line.Quantity,
                    SalesOrderLineId = line.Id,
                    SalesOrderLine = line,
                    CrDate = DateTime.UtcNow,
                    LcDate = DateTime.UtcNow,
                    CrUserId = userName,
                    LcUserId = userName
                };
                await _dispatchLineProcessor.AddAsync(dispatchLine, userName, null, true);
            }

            await _repository.AddAsync(dispatch);

            // loading with lazy loaded lines
            dispatch = await _repository.GetByIdAsync(dispatch.Id);

            return dispatch;
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
                await _dispatchLineProcessor.DeleteAsync(line.Id, userName);
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

        public async Task BlockAsyncInternal(int id, string userName)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            entity.BlDate = DateTime.UtcNow;
            entity.BlUserId = userName;

            foreach (var line in entity.Lines)
            {
                await _dispatchLineProcessor.BlockAsync(line.Id, userName);
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

        public async Task UnblockAsyncInternal(int id, string userName)
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
                await _dispatchLineProcessor.UnblockAsync(line.Id, userName);
            }
            await _repository.UpdateAsync(entity);
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
