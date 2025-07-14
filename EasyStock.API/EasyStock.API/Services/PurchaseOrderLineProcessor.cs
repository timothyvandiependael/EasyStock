using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;

namespace EasyStock.API.Services
{
    public class PurchaseOrderLineProcessor : IPurchaseOrderLineProcessor
    {
        private readonly IRepository<PurchaseOrderLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;

        public PurchaseOrderLineProcessor(IRepository<PurchaseOrderLine> repository, IRepository<Product> genericProductRepository)
        {
            _repository = repository;
            _genericProductRepository = genericProductRepository;
        }

        public async Task AddAsync(PurchaseOrderLine entity, string userName, Func<int, Task<int>>? getNextLineNumberAsync, bool fromParent = false)
        {
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;
            entity.Status = OrderStatus.Open;
            if (!fromParent && getNextLineNumberAsync != null)
                entity.LineNumber = await getNextLineNumberAsync(entity.PurchaseOrderId);
            await _repository.AddAsync(entity);

            var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {entity.ProductId} not found when updating inbound stock.");
            product.InboundStock += entity.Quantity;
            product.LcUserId = userName;
            product.LcDate = DateTime.UtcNow;
        }

        public async Task DeleteAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
                throw new InvalidOperationException($"Unable to delete record with ID {id}");
            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating inbound stock.");
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            record.BlDate = DateTime.UtcNow;
            record.BlUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating inbound stock.");
                product.InboundStock -= record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(record);
        }

        public async Task UnblockAsync(int id, string userName)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            record.BlDate = null;
            record.BlUserId = null;
            record.LcDate = DateTime.UtcNow;
            record.LcUserId = userName;

            if (record.Status == OrderStatus.Open || record.Status == OrderStatus.Partial)
            {
                var product = await _genericProductRepository.GetByIdAsync(record.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Product with ID {record.ProductId} not found when updating inbound stock.");
                product.InboundStock += record.Quantity;
                product.LcUserId = userName;
                product.LcDate = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(record);
        }
    }
}
