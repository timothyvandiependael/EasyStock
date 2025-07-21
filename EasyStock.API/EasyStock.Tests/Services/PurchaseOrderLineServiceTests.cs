using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EasyStock.API.Models;
using EasyStock.API.Services;
using EasyStock.API.Repositories;
using EasyStock.API.Common;
using EasyStock.Tests.TestHelpers;

namespace EasyStock.Tests.Services
{
    public class PurchaseOrderLineServiceTests
    {
        private readonly Mock<IPurchaseOrderLineRepository> _poLineRepoMock = new();
        private readonly Mock<IRepository<PurchaseOrderLine>> _repoMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRetryableTransactionService> _transactionMock = new();
        private readonly Mock<IPurchaseOrderService> _poServiceMock = new();
        private readonly Mock<IPurchaseOrderLineProcessor> _processorMock = new();
        private readonly EntityFactory _entityFactory = new();

        private readonly PurchaseOrderLineService _service;

        public PurchaseOrderLineServiceTests()
        {
            _service = new PurchaseOrderLineService(
                _poLineRepoMock.Object,
                _repoMock.Object,
                _transactionMock.Object,
                _productRepoMock.Object,
                _poServiceMock.Object,
                _processorMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_QuantityChangedAndStatusOpen_ShouldUpdateInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Id = 1;
            line.Status = OrderStatus.Open;
            line.Quantity = 10;

            var oldRecord = _entityFactory.CreatePurchaseOrderLine();
            oldRecord.Id = line.Id;
            oldRecord.Status = OrderStatus.Open;
            oldRecord.Quantity = 5;

            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _repoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(oldRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _service.UpdateAsync(line, "tester", useTransaction: false);

            // Assert
            Assert.Equal(originalStock + (line.Quantity - oldRecord.Quantity), product.InboundStock);
            Assert.Equal("tester", product.LcUserId);
            _repoMock.Verify(r => r.AddAsync(It.Is<PurchaseOrderLine>(p => p == line)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuantityUnchanged_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Id = 1;
            line.Status = OrderStatus.Open;
            line.Quantity = 10;

            var oldRecord = _entityFactory.CreatePurchaseOrderLine();
            oldRecord.Id = line.Id;
            oldRecord.Status = OrderStatus.Open;
            oldRecord.Quantity = 10;

            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _repoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(oldRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _service.UpdateAsync(line, "tester", useTransaction: false);

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
            _repoMock.Verify(r => r.AddAsync(It.Is<PurchaseOrderLine>(p => p == line)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_StatusComplete_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Id = 1;
            line.Status = OrderStatus.Complete;
            line.Quantity = 20;

            var oldRecord = _entityFactory.CreatePurchaseOrderLine();
            oldRecord.Id = line.Id;
            oldRecord.Status = OrderStatus.Complete;
            oldRecord.Quantity = 5;

            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _repoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(oldRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _service.UpdateAsync(line, "tester", useTransaction: false);

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
            _repoMock.Verify(r => r.AddAsync(It.Is<PurchaseOrderLine>(p => p == line)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Id = 99;

            _repoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync((PurchaseOrderLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateAsync(line, "tester", useTransaction: false));
        }

        [Fact]
        public async Task UpdateAsync_ProductNotFound_ShouldThrow()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Id = 1;
            line.Status = OrderStatus.Open;
            line.Quantity = 15;

            var oldRecord = _entityFactory.CreatePurchaseOrderLine();
            oldRecord.Id = line.Id;
            oldRecord.Status = OrderStatus.Open;
            oldRecord.Quantity = 10;

            _repoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(oldRecord);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateAsync(line, "tester", useTransaction: false));
        }
    }
}
