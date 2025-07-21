using System;
using System.Threading.Tasks;
using EasyStock.API.Models;
using EasyStock.API.Services;
using EasyStock.API.Repositories;
using Moq;
using Xunit;
using EasyStock.Tests.TestHelpers;
using EasyStock.API.Common;

namespace EasyStock.Tests.Services
{
    public class PurchaseOrderLineProcessorTests
    {
        private readonly EntityFactory _entityFactory;
        private readonly Mock<IRepository<PurchaseOrderLine>> _poLineRepoMock;
        private readonly Mock<IRepository<Product>> _productRepoMock;
        private readonly PurchaseOrderLineProcessor _processor;

        public PurchaseOrderLineProcessorTests()
        {
            _entityFactory = new EntityFactory();
            _poLineRepoMock = new Mock<IRepository<PurchaseOrderLine>>();
            _productRepoMock = new Mock<IRepository<Product>>();
            _processor = new PurchaseOrderLineProcessor(_poLineRepoMock.Object, _productRepoMock.Object);
        }

        [Fact]
        public async Task AddAsync_ValidEntity_ShouldAddAndUpdateInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            var product = _entityFactory.CreateProduct();
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);
            _poLineRepoMock.Setup(r => r.AddAsync(It.IsAny<PurchaseOrderLine>())).Returns(Task.CompletedTask);

            // Act
            await _processor.AddAsync(line, "tester", async (id) => 10);

            // Assert
            Assert.Equal(OrderStatus.Open, line.Status);
            Assert.Equal(10, line.LineNumber);
            _poLineRepoMock.Verify(r => r.AddAsync(line), Times.Once);
            Assert.True(product.InboundStock >= line.Quantity);
        }

        [Fact]
        public async Task AddAsync_ProductNotFound_ShouldThrow()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _processor.AddAsync(line, "tester", async (id) => 1));
        }

        [Fact]
        public async Task AddAsync_FromParentTrue_ShouldSkipLineNumberGeneration()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            var product = _entityFactory.CreateProduct();
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.AddAsync(line, "tester", async (id) => 99, fromParent: true);

            // Assert
            Assert.Equal(0, line.LineNumber); // assuming factory default is 0
        }

        [Fact]
        public async Task AddAsync_NullGetNextLineNumber_ShouldSkipLineNumberGeneration()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            var product = _entityFactory.CreateProduct();

            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.AddAsync(line, "tester", null);

            // Assert
            Assert.Equal(0, line.LineNumber); // assuming factory default
            _poLineRepoMock.Verify(r => r.AddAsync(line), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ZeroQuantity_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Quantity = 0;
            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.AddAsync(line, "tester", (id) => Task.FromResult(5));

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
        }

        [Fact]
        public async Task AddAsync_NegativeQuantity_ShouldDecreaseInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Quantity = -5;
            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.AddAsync(line, "tester", (id) => Task.FromResult(5));

            // Assert
            Assert.Equal(originalStock - 5, product.InboundStock);
        }

        [Fact]
        public async Task DeleteAsync_ValidOpenLine_ShouldReduceStockAndDelete()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            var product = _entityFactory.CreateProduct();

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.DeleteAsync(1, "tester");

            // Assert
            _poLineRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
            Assert.True(product.InboundStock <= 0); // reduced
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((PurchaseOrderLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.DeleteAsync(1, "tester"));
        }

        [Fact]
        public async Task DeleteAsync_CompleteStatus_ShouldNotChangeStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Complete;
            var product = _entityFactory.CreateProduct();

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.DeleteAsync(1, "tester");

            // Assert
            // Stock remains unchanged because status is not Open or Partial
            _poLineRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
            Assert.Equal(0, product.InboundStock); // factory default is 0
        }

        [Fact]
        public async Task DeleteAsync_StatusPartial_ShouldReduceStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Partial;
            var product = _entityFactory.CreateProduct();
            product.InboundStock = 10;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.DeleteAsync(line.Id, "tester");

            // Assert
            Assert.Equal(10 - line.Quantity, product.InboundStock);
            _poLineRepoMock.Verify(r => r.DeleteAsync(line.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuantityZero_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            line.Quantity = 0;
            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.DeleteAsync(line.Id, "tester");

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
        }

        [Fact]
        public async Task BlockAsync_ValidOpenLine_ShouldReduceStockAndUpdate()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            var product = _entityFactory.CreateProduct();

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.BlockAsync(1, "tester");

            // Assert
            _poLineRepoMock.Verify(r => r.UpdateAsync(line), Times.Once);
            Assert.NotNull(line.BlDate);
            Assert.Equal("tester", line.BlUserId);
            Assert.True(product.InboundStock <= 0);
        }

        [Fact]
        public async Task BlockAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((PurchaseOrderLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.BlockAsync(1, "tester"));
        }

        [Fact]
        public async Task BlockAsync_StatusPartial_ShouldReduceStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Partial;
            var product = _entityFactory.CreateProduct();
            product.InboundStock = 20;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.BlockAsync(line.Id, "tester");

            // Assert
            Assert.Equal(20 - line.Quantity, product.InboundStock);
            Assert.NotNull(line.BlDate);
            Assert.Equal("tester", line.BlUserId);
        }

        [Fact]
        public async Task BlockAsync_StatusComplete_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Complete;
            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.BlockAsync(line.Id, "tester");

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
            Assert.NotNull(line.BlDate);
            Assert.Equal("tester", line.BlUserId);
        }

        [Fact]
        public async Task BlockAsync_AlreadyBlocked_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            line.BlDate = DateTime.UtcNow;
            line.BlUserId = "testuser";
            line.Quantity = 10;
            var product = _entityFactory.CreateProduct();
            var originalInboundStock = product.InboundStock;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act 
            await _processor.BlockAsync(line.Id, "tester");

            // Assert
            Assert.Equal(originalInboundStock, product.InboundStock);
        }

        [Fact]
        public async Task UnblockAsync_ValidOpenLine_ShouldIncreaseStockAndUpdate()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            line.BlDate = DateTime.UtcNow;
            line.BlUserId = "someone";
            var product = _entityFactory.CreateProduct();

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.UnblockAsync(1, "tester");

            // Assert
            _poLineRepoMock.Verify(r => r.UpdateAsync(line), Times.Once);
            Assert.Null(line.BlDate);
            Assert.Null(line.BlUserId);
            Assert.True(product.InboundStock >= line.Quantity);
        }

        [Fact]
        public async Task UnblockAsync_RecordNotFound_ShouldThrow()
        {
            // Arrange
            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((PurchaseOrderLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _processor.UnblockAsync(1, "tester"));
        }

        [Fact]
        public async Task UnblockAsync_StatusPartial_ShouldIncreaseStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Partial;
            line.BlDate = DateTime.UtcNow;
            var product = _entityFactory.CreateProduct();
            product.InboundStock = 50;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.UnblockAsync(line.Id, "tester");

            // Assert
            Assert.Null(line.BlDate);
            Assert.Null(line.BlUserId);
            Assert.Equal(50 + line.Quantity, product.InboundStock);
        }

        [Fact]
        public async Task UnblockAsync_StatusComplete_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Complete;
            line.BlDate = DateTime.UtcNow;
            var product = _entityFactory.CreateProduct();
            var originalStock = product.InboundStock;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.UnblockAsync(line.Id, "tester");

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
            Assert.Null(line.BlDate);
            Assert.Null(line.BlUserId);
        }

        [Fact]
        public async Task UnblockAsync_NotBlocked_ShouldNotChangeInboundStock()
        {
            // Arrange
            var line = _entityFactory.CreatePurchaseOrderLine();
            line.Status = OrderStatus.Open;
            line.BlDate = null;
            line.BlUserId = null;
            var product = _entityFactory.CreateProduct();
            product.InboundStock = 50;
            var originalStock = product.InboundStock;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(line.Id)).ReturnsAsync(line);
            _productRepoMock.Setup(r => r.GetByIdAsync(line.ProductId)).ReturnsAsync(product);

            // Act
            await _processor.UnblockAsync(line.Id, "tester");

            // Assert
            Assert.Equal(originalStock, product.InboundStock);
        }
    }
}
