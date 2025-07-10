using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EasyStock.API.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IRetryableTransactionService _retryableTransactionService;
        private readonly IOrderNumberCounterService _orderNumberCounterService;
        private readonly IRepository<SalesOrder> _repository;
        private readonly ISalesOrderLineService _salesOrderLineService;

        public SalesOrderService(ISalesOrderRepository salesOrderRepository, IRetryableTransactionService retryableTransactionService, IOrderNumberCounterService orderNumberCounterService, IRepository<SalesOrder> repository,
            ISalesOrderLineService salesOrderLineService)
        {
            _salesOrderRepository = salesOrderRepository;
            _retryableTransactionService = retryableTransactionService;
            _repository = repository;
            _orderNumberCounterService = orderNumberCounterService;
            _salesOrderLineService = salesOrderLineService;
        }

        public async Task<IEnumerable<SalesOrderOverview>> GetAllAsync()
            => await _salesOrderRepository.GetAllAsync();

        public async Task<PaginationResult<SalesOrderOverview>> GetAdvancedAsync(List<FilterCondition> filters, List<SortOption> sorting, Pagination pagination)
            => await _salesOrderRepository.GetAdvancedAsync(filters, sorting, pagination);

        public async Task AddAsync(SalesOrder entity, string userName)
        {
            entity.OrderNumber = await _orderNumberCounterService.GenerateOrderNumberAsync(OrderType.SalesOrder);
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
                    await _salesOrderLineService.DeleteAsync(line.Id, userName, false);
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
                    await _salesOrderLineService.BlockAsync(line.Id, userName, false);
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
                    await _salesOrderLineService.UnblockAsync(line.Id, userName, false);
                }
                await _repository.UpdateAsync(entity);
            });
        }

        public async Task<List<Product>> GetProductsWithSuppliersForOrderAsync(int id)
        {
            var products = new List<Product>();
            var so = await _repository.GetByIdAsync(id);
            if (so == null) throw new Exception($"Salesorder with id {id} not found.");
            foreach (var line in so.Lines)
            {
                products.Add(line.Product);
            }

            return products;
        }

        public async Task<int> GetNextLineNumberAsync(int id)
        {
            var so = await _repository.GetByIdAsync(id);
            if (so == null) throw new Exception($"Salesorder with id {id} not found.");
            var nextLineNumber = so.Lines.Any() ? so.Lines.Max(l => l.LineNumber) + 1 : 1;
            return nextLineNumber;
        }

    }
}
