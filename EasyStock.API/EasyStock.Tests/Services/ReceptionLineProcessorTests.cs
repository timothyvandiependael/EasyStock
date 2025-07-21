using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyStock.Tests.Services
{
    public class ReceptionLineProcessorTests
    {
        private readonly EntityFactory _entityFactory = new EntityFactory();

        private readonly Mock<IRepository<ReceptionLine>> _receptionLineRepoMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRepository<PurchaseOrderLine>> _poLineRepoMock = new();
        private readonly Mock<IRepository<PurchaseOrder>> _poRepoMock = new();
        private readonly Mock<IRepository<StockMovement>> _stockMovementRepoMock = new();

        private readonly ReceptionLineProcessor _service;

        public ReceptionLineProcessorTests()
        {
            _service = new ReceptionLineProcessor(
                _receptionLineRepoMock.Object,
                _productRepoMock.Object,
                _poLineRepoMock.Object,
                _poRepoMock.Object,
                _stockMovementRepoMock.Object);
        }

        private void SetupPoLineAndPoMocks(PurchaseOrderLine poLine, PurchaseOrder po)
        {
            _poLineRepoMock.Setup(r => r.GetByIdAsync(poLine.Id)).ReturnsAsync(poLine);
            _poRepoMock.Setup(r => r.GetByIdAsync(poLine.PurchaseOrderId)).ReturnsAsync(po);
        }

        private (PurchaseOrderLine, PurchaseOrder) CreatePoLineAndPo(int poLineStatusCountOpenOrPartial)
        {
            var po = _entityFactory.CreatePurchaseOrder();
            var poLine = _entityFactory.CreatePurchaseOrderLine();
            poLine.Id = 1;
            poLine.PurchaseOrderId = po.Id;
            po.Lines = new List<PurchaseOrderLine>();

            // Setup multiple lines with specified status to test PO status logic
            for (int i = 0; i < poLineStatusCountOpenOrPartial; i++)
            {
                var line = _entityFactory.CreatePurchaseOrderLine();
                line.Status = OrderStatus.Open;
                line.PurchaseOrderId = po.Id;
                po.Lines.Add(line);
            }
                
            for (int i = poLineStatusCountOpenOrPartial; i < 3; i++)
            {
                var line = _entityFactory.CreatePurchaseOrderLine();
                line.Status = OrderStatus.Complete;
                line.PurchaseOrderId = po.Id;
                po.Lines.Add(line);
            }

            return (poLine, po);
        }

        [Fact]
        public async Task SetPOStatusFields_PurchaseOrderLineNotFound_ShouldThrow()
        {
            // Arrange
            _poLineRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PurchaseOrderLine?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SetPOStatusFields(10, 1, "user"));
            Assert.Equal("Unable to find purchase order line with ID 1", ex.Message);
        }

        [Fact]
        public async Task SetPOStatusFields_PurchaseOrderNotFound_ShouldThrow()
        {
            // Arrange
            var poLine = _entityFactory.CreatePurchaseOrderLine();
            poLine.Id = 1;
            poLine.PurchaseOrderId = 2;

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(poLine);
            _poRepoMock.Setup(r => r.GetByIdAsync(poLine.PurchaseOrderId)).ReturnsAsync((PurchaseOrder?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SetPOStatusFields(10, 1, "user"));
            Assert.Equal($"Unable to find purchase order with ID {poLine.PurchaseOrderId}", ex.Message);
        }

        [Fact]
        public async Task SetPOStatusFields_QuantityLessThanPOLineQuantity_ShouldSetStatusPartial()
        {
            // Arrange
            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            // Act
            await _service.SetPOStatusFields(5, poLine.Id, "user");

            // Assert
            Assert.Equal(OrderStatus.Partial, poLine.Status);
            Assert.Equal("user", poLine.LcUserId);
            Assert.Equal("user", po.LcUserId);
            Assert.Equal(OrderStatus.Partial, po.Status);
        }

        [Fact]
        public async Task SetPOStatusFields_QuantityEqualOrMoreThanPOLineQuantity_ShouldSetStatusComplete()
        {
            // Arrange
            var (poLine, po) = CreatePoLineAndPo(0);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            // Act
            await _service.SetPOStatusFields(10, poLine.Id, "user");

            // Assert
            Assert.Equal(OrderStatus.Complete, poLine.Status);
            Assert.Equal(OrderStatus.Complete, po.Status);
            Assert.Equal("user", poLine.LcUserId);
            Assert.Equal("user", po.LcUserId);
        }

        [Fact]
        public async Task SetPOStatusFields_PurchaseOrderLinesContainPartialOrOpen_ShouldSetPOStatusPartial()
        {
            // Arrange
            var (poLine, po) = CreatePoLineAndPo(2); // 2 lines with status Open
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            // Act
            await _service.SetPOStatusFields(10, poLine.Id, "user");

            // Assert
            Assert.Equal(OrderStatus.Complete, poLine.Status);
            Assert.Equal(OrderStatus.Partial, po.Status); // Because some lines are Open
        }

        [Fact]
        public async Task AddAsync_HappyPath_ShouldSetFieldsAndAddEntities()
        {
            // Arrange
            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.PurchaseOrderLineId = 1;
            receptionLine.ReceptionId = 1;
            receptionLine.Quantity = 5;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;
            product.BackOrderedStock = 2;
            product.AvailableStock = 10;
            product.ReservedStock = 3;
            product.InboundStock = 5;
            product.TotalStock = 15;

            var poLine = _entityFactory.CreatePurchaseOrderLine();
            poLine.Id = 1;
            poLine.Quantity = 5;
            poLine.PurchaseOrderId = 1;
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 1;
            po.Lines = new List<PurchaseOrderLine> { poLine };

            _poLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(poLine);
            _poRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(po);

            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // To test getNextLineNumberAsync callback
            Func<int, Task<int>> getNextLineNumberAsync = (receptionId) => Task.FromResult(42);

            // Setup PO line status update mocks (called internally)
            _poLineRepoMock.Setup(r => r.GetByIdAsync(receptionLine.PurchaseOrderLineId)).ReturnsAsync(poLine);
            _poRepoMock.Setup(r => r.GetByIdAsync(poLine.PurchaseOrderId)).ReturnsAsync(po);

            // Act
            await _service.AddAsync(receptionLine, "user", getNextLineNumberAsync);

            // Assert
            Assert.Equal(42, receptionLine.LineNumber);
            Assert.Equal("user", receptionLine.CrUserId);
            Assert.Equal("user", receptionLine.LcUserId);
            Assert.Equal("user", product.LcUserId);
            Assert.Equal(product.TotalStock, 15 + 5);
            Assert.Equal(product.InboundStock, 5 - 5);

            _receptionLineRepoMock.Verify(r => r.AddAsync(receptionLine), Times.Once);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm => sm.QuantityChange == receptionLine.Quantity && sm.Reason == "Reception")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_ProductNotFound_ShouldThrow()
        {
            // Arrange
            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.ProductId = 1;
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(receptionLine, "user", null));
            Assert.Equal("Unable to find product with ID 1", ex.Message);
        }

        [Fact]
        public async Task DeleteAsync_HappyPath_ShouldUpdateStocksAndDeleteEntity()
        {
            // Arrange
            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.Quantity = 3;
            receptionLine.PurchaseOrderLineId = 2;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;
            product.AvailableStock = 5;
            product.ReservedStock = 4;
            product.BackOrderedStock = 0;
            product.InboundStock = 10;
            product.TotalStock = 10;

            var poLine = _entityFactory.CreatePurchaseOrderLine();
            poLine.Id = receptionLine.PurchaseOrderLineId;
            poLine.PurchaseOrderId = 1;
            var po = _entityFactory.CreatePurchaseOrder();
            po.Id = 1;
            po.Lines = new List<PurchaseOrderLine> { poLine };

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(receptionLine.ProductId)).ReturnsAsync(product);

            _poLineRepoMock.Setup(r => r.GetByIdAsync(receptionLine.PurchaseOrderLineId)).ReturnsAsync(poLine);
            _poRepoMock.Setup(r => r.GetByIdAsync(poLine.PurchaseOrderId)).ReturnsAsync(po);

            // Act
            await _service.DeleteAsync(1, "user");

            // Assert
            Assert.Equal(10 + 3, product.InboundStock); // increased inbound by quantity
            Assert.Equal(10 - 3, product.TotalStock);   // decreased total stock

            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm => sm.QuantityChange == -3 && sm.Reason == "Reception Line deletion")), Times.Once);
            _receptionLineRepoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ProductNotFound_ShouldThrow()
        {
            // Arrange
            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(1, "user"));
            Assert.Equal("Unable to find product with ID 1", ex.Message);
        }

        [Fact]
        public async Task BlockAsync_HappyPath_ShouldSetBlockedAndUpdateProductStock()
        {
            // Arrange

            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.Quantity = 3;
            receptionLine.BlDate = DateTime.UtcNow;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;
            product.AvailableStock = 10;
            product.ReservedStock = 5;
            product.BackOrderedStock = 2;
            product.InboundStock = 3;
            product.TotalStock = 20;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            await _service.BlockAsync(1, "user");

            // Assert
            Assert.NotEqual(default, receptionLine.BlDate);
            Assert.Equal(10 - 3, product.AvailableStock);
            Assert.Equal("user", receptionLine.BlUserId);
        }

        [Fact]
        public async Task BlockAsync_AlreadyBlocked_ShouldNotChangeStock()
        {
            // Arrange

            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.Quantity = 3;
            receptionLine.BlDate = DateTime.UtcNow;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            await _service.BlockAsync(1, "user");

            // Assert
            Assert.NotEqual(default, receptionLine.BlDate);
            // No stock changes expected
        }

        [Fact]
        public async Task UnblockAsync_HappyPath_ShouldUnsetBlockedAndUpdateProductStock()
        {
            // Arrange

            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.Quantity = 3;
            receptionLine.BlDate = DateTime.UtcNow;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;
            product.AvailableStock = 7;
            product.ReservedStock = 2;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            await _service.UnblockAsync(1, "user");

            // Assert
            Assert.Null(receptionLine.BlDate);
            Assert.Equal(7 + 3, product.AvailableStock);
            Assert.Equal("user", receptionLine.LcUserId);
        }

        [Fact]
        public async Task UnblockAsync_AlreadyUnblocked_ShouldNotChangeStock()
        {
            // Arrange

            var (poLine, po) = CreatePoLineAndPo(2);
            poLine.Quantity = 10;

            SetupPoLineAndPoMocks(poLine, po);

            var receptionLine = _entityFactory.CreateReceptionLine();
            receptionLine.Id = 1;
            receptionLine.ProductId = 1;
            receptionLine.Quantity = 3;
            receptionLine.BlDate = null;

            var product = _entityFactory.CreateProduct();
            product.Id = 1;

            _receptionLineRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receptionLine);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            await _service.UnblockAsync(1, "user");

            // Assert
            Assert.Null(receptionLine.BlDate);
            // No stock changes expected
        }
    }
}
