using System;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;

namespace EasyStock.Tests.Services
{
    public class DispatchLineServiceTests
    {
        private readonly Mock<IDispatchLineRepository> _dispatchLineRepoMock = new();
        private readonly Mock<IRetryableTransactionService> _retryableTransactionServiceMock = new();
        private readonly Mock<IRepository<DispatchLine>> _repositoryMock = new();
        private readonly Mock<IRepository<Product>> _productRepoMock = new();
        private readonly Mock<IRepository<SalesOrder>> _salesOrderRepoMock = new();
        private readonly Mock<IRepository<SalesOrderLine>> _salesOrderLineRepoMock = new();
        private readonly Mock<IDispatchService> _dispatchServiceMock = new();
        private readonly Mock<IRepository<StockMovement>> _stockMovementRepoMock = new();
        private readonly Mock<IDispatchLineProcessor> _dispatchLineProcessorMock = new();

        private readonly DispatchLineService _service;

        private readonly DispatchLine _dispatchLine;
        private readonly Product _product;
        private readonly Client _client;

        public DispatchLineServiceTests()
        {
            _service = new DispatchLineService(
                _dispatchLineRepoMock.Object,
                _retryableTransactionServiceMock.Object,
                _repositoryMock.Object,
                _productRepoMock.Object,
                _salesOrderRepoMock.Object,
                _salesOrderLineRepoMock.Object,
                _dispatchServiceMock.Object,
                _stockMovementRepoMock.Object,
                _dispatchLineProcessorMock.Object
            );

            _client = new Client
            {
                Id = 1,
                CrDate = new DateTime(2024, 1, 1),
                CrUserId = "creator",
                LcDate = new DateTime(2024, 1, 1),
                LcUserId = "creator"
            };

            _product = new Product
            {
                Id = 2,
                Category = new Category {
                    Id = 1,
                    Name = "testcategory",
                    CrDate = new DateTime(2024, 1, 1),
                    CrUserId = "creator",
                    LcDate = new DateTime(2024, 1, 1),
                    LcUserId = "creator"
                },
                ReservedStock = 100,
                TotalStock = 200,
                CrDate = new DateTime(2024, 1, 1),
                CrUserId = "creator",
                LcDate = new DateTime(2024, 1, 1),
                LcUserId = "creator"
            };

            _dispatchLine = new DispatchLine
            {
                Id = 1,
                Quantity = 5,
                ProductId = 2,
                SalesOrderLineId = 3,
                DispatchId = 1,
                Product = _product,
                SalesOrderLine = new SalesOrderLine
                {
                    SalesOrderId = 10,
                    SalesOrder = new SalesOrder
                    {
                        OrderNumber = "test",
                        Client = _client,
                        CrDate = new DateTime(2024, 1, 1),
                        CrUserId = "creator",
                        LcDate = new DateTime(2024, 1, 1),
                        LcUserId = "creator"
                    },
                    Product = _product,
                    ProductId = _product.Id,
                    CrDate = new DateTime(2024, 1, 1),
                    CrUserId = "creator",
                    LcDate = new DateTime(2024, 1, 1),
                    LcUserId = "creator"
                },
                Dispatch = new Dispatch
                {
                    Id = 1,
                    DispatchNumber = "test",
                    Client = _client,
                    CrDate = new DateTime(2024, 1, 1),
                    CrUserId = "creator",
                    LcDate = new DateTime(2024, 1, 1),
                    LcUserId = "creator"
                },
                CrDate = new DateTime(2024, 1, 1),
                CrUserId = "creator",
                LcDate = new DateTime(2024, 1, 1),
                LcUserId = "creator"
            };
        }

        private static T DeepClone<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFieldsAndCreateStockMovement_WhenQuantityChanges()
        {
            // Arrange
            var entity = DeepClone(_dispatchLine);
            entity.Quantity = 7; // changed

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync(DeepClone(_dispatchLine));

            _productRepoMock.Setup(r => r.GetByIdAsync(entity.ProductId))
                .ReturnsAsync(DeepClone(_product));

            // Act
            await _service.UpdateAsyncProcess(entity, "tester");
            Console.WriteLine($"entity.LcUserId = {entity.LcUserId}");

            // Assert
            Assert.Equal("tester", entity.LcUserId);

            _dispatchLineProcessorMock.Verify(p => p.SetSOStatusFields(entity.Quantity, entity.SalesOrderLineId, "tester"), Times.Once);

            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == entity.ProductId &&
                sm.QuantityChange == 0 - (entity.Quantity - _dispatchLine.Quantity) &&
                sm.Reason == "Correction of dispatch line" &&
                sm.CrUserId == "tester"
            )), Times.Once);

            _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldOnlyUpdateFields_WhenQuantityUnchanged()
        {
            // Arrange
            var entity = DeepClone(_dispatchLine); // same quantity

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync(DeepClone(_dispatchLine));

            // Act
            await _service.UpdateAsyncProcess(entity, "tester");

            // Assert
            Assert.Equal("tester", entity.LcUserId);
            Assert.NotEqual(entity.CrDate, entity.LcDate);

            _dispatchLineProcessorMock.Verify(p => p.SetSOStatusFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
            _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenOriginalRecordNotFound()
        {
            // Arrange
            var entity = DeepClone(_dispatchLine);

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync((DispatchLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsyncProcess(entity, "tester"));

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DispatchLine>()), Times.Never);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
            _dispatchLineProcessorMock.Verify(p => p.SetSOStatusFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
