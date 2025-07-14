using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EasyStock.API.Services
{
    public class ReceptionLineProcessor : IReceptionLineProcessor
    {
        private readonly IRepository<ReceptionLine> _repository;
        private readonly IRepository<Product> _genericProductRepository;
        private readonly IRepository<PurchaseOrderLine> _genericPurchaseOrderLineRepository;
        private readonly IRepository<PurchaseOrder> _genericPurchaseOrderRepository;
        private readonly IRepository<StockMovement> _genericStockMovementRepository;

        public ReceptionLineProcessor(IRepository<ReceptionLine> repository, IRepository<Product> genericProductRepository, IRepository<PurchaseOrderLine> genericPurchaseOrderLineRepository, IRepository<PurchaseOrder> genericPurchaseOrderRepository, IRepository<StockMovement> genericStockMovementRepository)
        {
            _repository = repository;
            _genericProductRepository = genericProductRepository;
            _genericPurchaseOrderLineRepository = genericPurchaseOrderLineRepository;
            _genericPurchaseOrderRepository = genericPurchaseOrderRepository;
            _genericStockMovementRepository = genericStockMovementRepository;
        }

        public async Task SetPOStatusFields(int quantity, int purchaseOrderLineId, string userName)
        {
            var poLine = await _genericPurchaseOrderLineRepository.GetByIdAsync(purchaseOrderLineId);
            if (poLine == null)
                throw new InvalidOperationException($"Unable to find purchase order line with ID {purchaseOrderLineId}");
            poLine.LcDate = DateTime.UtcNow;
            poLine.LcUserId = userName;
            if (quantity < poLine.Quantity)
            {
                poLine.Status = OrderStatus.Partial;
            }

            else
            {
                poLine.Status = OrderStatus.Complete;
            }

            var po = await _genericPurchaseOrderRepository.GetByIdAsync(poLine.PurchaseOrderId);
            if (po == null)
                throw new InvalidOperationException($"Unable to find purchase order with ID {poLine.PurchaseOrderId}");
            po.LcDate = DateTime.UtcNow;
            po.LcUserId = userName;
            if (po.Lines.Any(l => l.Status == OrderStatus.Partial || l.Status == OrderStatus.Open))
            {
                po.Status = OrderStatus.Partial;
            }
            else
            {
                po.Status = OrderStatus.Complete;
            }
        }

        public async Task AddAsync(ReceptionLine entity, string userName, Func<int, Task<int>>? getNextLineNumberAsync, bool fromParent = false)
        {
            entity.CrDate = DateTime.UtcNow;
            entity.LcDate = entity.CrDate;
            entity.CrUserId = userName;
            entity.LcUserId = userName;
            if (!fromParent && getNextLineNumberAsync != null)
                entity.LineNumber = await getNextLineNumberAsync(entity.ReceptionId);

            await SetPOStatusFields(entity.Quantity, entity.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(entity.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {entity.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock -= entity.Quantity;
            product.TotalStock += entity.Quantity;

            product.AvailableStock +=
                product.BackOrderedStock > entity.Quantity
                ? 0
                : (entity.Quantity - product.BackOrderedStock);

            product.ReservedStock +=
                product.BackOrderedStock > entity.Quantity
                ? entity.Quantity
                : product.BackOrderedStock;

            if (product.BackOrderedStock > 0)
            {
                product.BackOrderedStock -=
                    product.BackOrderedStock > entity.Quantity
                    ? entity.Quantity
                    : product.BackOrderedStock;
            }

            await _repository.AddAsync(entity);

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = entity.Quantity,
                Reason = "Reception",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                PurchaseOrderId = entity.PurchaseOrderLine.PurchaseOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);
        }

        public async Task DeleteAsync(int id, string userName)
        {
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to find record with ID {id}");

            await SetPOStatusFields(0, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock += receptionLine.Quantity;
            product.TotalStock -= receptionLine.Quantity;

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = 0 - receptionLine.Quantity,
                Reason = "Reception Line deletion",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                PurchaseOrderId = receptionLine.PurchaseOrderLine.PurchaseOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);

            var tmpAvailableStock = product.AvailableStock - receptionLine.Quantity;
            if (tmpAvailableStock < 0) // Meaning it came originally from backorder
            {
                var stockShortage = Math.Abs(tmpAvailableStock);
                product.AvailableStock = 0;
                product.ReservedStock -= stockShortage;
                product.BackOrderedStock += stockShortage;
            }
            else
            {
                product.AvailableStock -= receptionLine.Quantity;
            }

            await _repository.DeleteAsync(id);
        }

        public async Task BlockAsync(int id, string userName)
        {
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to block record with ID {id}");
            receptionLine.BlDate = DateTime.UtcNow;
            receptionLine.BlUserId = userName;

            await SetPOStatusFields(0, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock += receptionLine.Quantity;
            product.TotalStock -= receptionLine.Quantity;

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = 0 - receptionLine.Quantity,
                Reason = "Reception Line blocked",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                PurchaseOrderId = receptionLine.PurchaseOrderLine.PurchaseOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);

            var tmpAvailableStock = product.AvailableStock - receptionLine.Quantity;
            if (tmpAvailableStock < 0) // Meaning it came originally from backorder
            {
                var stockShortage = Math.Abs(tmpAvailableStock);
                product.AvailableStock = 0;
                product.ReservedStock -= stockShortage;
                product.BackOrderedStock += stockShortage;
            }
            else
            {
                product.AvailableStock -= receptionLine.Quantity;
            }

            await _repository.UpdateAsync(receptionLine);
        }

        public async Task UnblockAsync(int id, string userName)
        {
            var receptionLine = await _repository.GetByIdAsync(id);
            if (receptionLine == null)
                throw new InvalidOperationException($"Unable to unblock record with ID {id}");
            receptionLine.BlDate = null;
            receptionLine.BlUserId = null;
            receptionLine.LcDate = DateTime.UtcNow;
            receptionLine.LcUserId = userName;

            await SetPOStatusFields(receptionLine.Quantity, receptionLine.PurchaseOrderLineId, userName);

            var product = await _genericProductRepository.GetByIdAsync(receptionLine.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Unable to find product with ID {receptionLine.ProductId}");
            product.LcDate = DateTime.UtcNow;
            product.LcUserId = userName;
            product.InboundStock -= receptionLine.Quantity;
            product.TotalStock += receptionLine.Quantity;

            var stockMovement = new StockMovement
            {
                ProductId = product.Id,
                Product = product,
                QuantityChange = receptionLine.Quantity,
                Reason = "Reception Line unblocked",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = userName,
                LcUserId = userName,
                PurchaseOrderId = receptionLine.PurchaseOrderLine.PurchaseOrderId
            };

            await _genericStockMovementRepository.AddAsync(stockMovement);

            product.AvailableStock +=
                    product.BackOrderedStock > receptionLine.Quantity
                    ? 0
                    : (receptionLine.Quantity - product.BackOrderedStock);

            product.ReservedStock +=
                product.BackOrderedStock > receptionLine.Quantity
                ? receptionLine.Quantity
                : product.BackOrderedStock;

            if (product.BackOrderedStock > 0)
            {
                product.BackOrderedStock -=
                    product.BackOrderedStock > receptionLine.Quantity
                    ? receptionLine.Quantity
                    : product.BackOrderedStock;
            }

            await _repository.UpdateAsync(receptionLine);
        }
    }
}
