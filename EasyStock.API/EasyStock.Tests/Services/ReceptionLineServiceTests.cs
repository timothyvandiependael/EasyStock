using System;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Services;
using EasyStock.API.Repositories;
using Moq;
using Xunit;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class ReceptionLineServiceTests
    {
        private readonly EntityFactory _entityFactory = new();

        private readonly Mock<IRepository<ReceptionLine>> _receptionLineRepoMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRepository<StockMovement>> _stockMovementRepoMock = new();
        private readonly Mock<IReceptionLineProcessor> _receptionLineProcessorMock = new();
        private readonly Mock<IRetryableTransactionService> _retryableTransactionMock = new();
        private readonly Mock<IReceptionLineRepository> _receptionLineRepositoryMock = new();
        private readonly Mock<IRepository<PurchaseOrderLine>> _purchaseOrderLineRepoMock = new();
        private readonly Mock<IRepository<PurchaseOrder>> _purchaseOrderRepoMock = new();
        private readonly Mock<IReceptionService> _receptionServiceMock = new();
        private readonly Mock<IUpdateService<ReceptionLine>> _updateServiceMock = new();

        private ReceptionLineService CreateService()
        {
            return new ReceptionLineService(
                _receptionLineRepositoryMock.Object,
                _retryableTransactionMock.Object,
                _purchaseOrderLineRepoMock.Object,
                _receptionLineRepoMock.Object,
                _productRepoMock.Object,
                _purchaseOrderRepoMock.Object,
                _receptionServiceMock.Object,
                _stockMovementRepoMock.Object,
                _receptionLineProcessorMock.Object,
                _updateServiceMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_QuantityChanged_ShouldUpdateReceptionLineAndProductStocks()
        {
            // Arrange
            var service = CreateService();

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.Quantity = 5;
            receptionLine.ProductId = 1;
            receptionLine.PurchaseOrderLineId = 1;
            receptionLine.PurchaseOrderLine = _entityFactory.CreatePurchaseOrderLine();
            receptionLine.PurchaseOrderLine.PurchaseOrderId = 10;
            receptionLine.CrDate = DateTime.UtcNow;

            var originalRecord = _entityFactory.CreateReceptionLine();
            originalRecord.Id = 1;
            originalRecord.Quantity = 3;
            originalRecord.ProductId = 1;
            originalRecord.PurchaseOrderLineId = 1;
            originalRecord.PurchaseOrderLine = receptionLine.PurchaseOrderLine;
            originalRecord.CrDate = DateTime.UtcNow.AddDays(-1);

            var product = _entityFactory.CreateProduct();
            product.Id = 1;
            product.AvailableStock = 10;
            product.ReservedStock = 5;
            product.BackOrderedStock = 4;
            product.InboundStock = 20;
            product.TotalStock = 30;

            int expectedAvailableStock = product.AvailableStock;
            int expectedReservedStock = product.ReservedStock;
            int backOrdered = product.BackOrderedStock;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(receptionLine.Id)).ReturnsAsync(originalRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            _stockMovementRepoMock.Setup(r => r.AddAsync(It.IsAny<StockMovement>())).Returns(Task.CompletedTask);
            _receptionLineRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ReceptionLine>())).Returns(Task.CompletedTask);

            // Act
            await service.UpdateAsync(receptionLine, "tester", false);

            // Assert
            _receptionLineRepoMock.Verify(r => r.GetByIdAsync(receptionLine.Id), Times.Once);
            _productRepoMock.Verify(r => r.GetByIdAsync(product.Id), Times.Once);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == product.Id &&
                sm.QuantityChange == (receptionLine.Quantity - originalRecord.Quantity) &&
                sm.PurchaseOrderId == receptionLine.PurchaseOrderLine.PurchaseOrderId)), Times.Once);

            // Stock adjustments
            int diff = receptionLine.Quantity - originalRecord.Quantity;
            if (diff > 0)
            {
                // AvailableStock and ReservedStock adjusted with backorder logic
                
                var expectedBackOrdered = backOrdered;
                if (backOrdered > 0)
                {
                    expectedBackOrdered = backOrdered - (backOrdered > diff ? diff : backOrdered);
                }

                expectedAvailableStock += backOrdered > diff ? 0 : (diff - backOrdered);
                expectedReservedStock += backOrdered > diff ? diff : backOrdered;

                Assert.Equal(expectedAvailableStock, product.AvailableStock);
                Assert.Equal(expectedReservedStock, product.ReservedStock);
                Assert.Equal(expectedBackOrdered, product.BackOrderedStock);
            }
        }

        [Fact]
        public async Task UpdateAsync_ReceptionLineNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();
            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 99;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(receptionLine.Id)).ReturnsAsync((ReceptionLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(receptionLine, "tester", false));
        }

        [Fact]
        public async Task UpdateAsync_ProductNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateService();

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.Quantity = 10;
            receptionLine.ProductId = 1;
            receptionLine.PurchaseOrderLineId = 1;
            receptionLine.PurchaseOrderLine = _entityFactory.CreatePurchaseOrderLine();

            var originalRecord = _entityFactory.CreateReceptionLine();
            originalRecord.Id = 1;
            originalRecord.Quantity = 5;
            originalRecord.ProductId = 1;
            originalRecord.PurchaseOrderLine = receptionLine.PurchaseOrderLine;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(receptionLine.Id)).ReturnsAsync(originalRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(receptionLine.ProductId)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(receptionLine, "tester", false));
        }
    }
}
