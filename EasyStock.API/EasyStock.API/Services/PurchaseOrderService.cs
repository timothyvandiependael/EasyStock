using EasyStock.API.Common;
using EasyStock.API.Data;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.API.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IRepository<PurchaseOrder> _repository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IPurchaseOrderLineService _purchaseOrderLineService;

        public PurchaseOrderService(IPurchaseOrderRepository purchaseOrderRepository, IOrderNumberCounterService orderNumberCounterService, IRepository<PurchaseOrder> repository, IRetryableTransactionService retryableTransactionService, IPurchaseOrderLineService purchaseOrderLineService)
        {
            _purchaseOrderRepository = purchaseOrderRepository;
            _orderNumberCounterService = orderNumberCounterService;
            _repository = repository;
            _retryableTransactionService = retryableTransactionService;
            _purchaseOrderLineService = purchaseOrderLineService;
        }

        public async Task<IEnumerable<PurchaseOrderOverview>> GetAllAsync()
            => await _purchaseOrderRepository.GetAllAsync();

        public async Task<PaginationResult<PurchaseOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _purchaseOrderRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(PurchaseOrder entity, string userName)
        {
            entity.OrderNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.PurchaseOrder);
            entity.Status = OrderStatus.Open;
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
                    await _purchaseOrderLineService.DeleteAsync(line.Id, userName, false);
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
                    await _purchaseOrderLineService.BlockAsync(line.Id, userName, false);
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
                    await _purchaseOrderLineService.UnblockAsync(line.Id, userName, false);
                }
                await _repository.UpdateAsync(entity);
            });
        }

    }
}
