using System;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;

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

        private readonly IDispatchLineService _service;
        private readonly EntityFactory _entityFactory;

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

            _entityFactory = new EntityFactory();
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

            var oldDispatchLine = _entityFactory.CreateDispatchLine();

            var entity = _entityFactory.CreateDispatchLine();
            entity.Quantity = 7; // changed

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync(_entityFactory.CreateDispatchLine());

            _productRepoMock.Setup(r => r.GetByIdAsync(entity.ProductId))
                .ReturnsAsync(_entityFactory.CreateProduct());

            // Act
            await _service.UpdateAsync(entity, "tester", false);
            Console.WriteLine($"entity.LcUserId = {entity.LcUserId}");

            // Assert
            Assert.Equal("tester", entity.LcUserId);

            _dispatchLineProcessorMock.Verify(p => p.SetSOStatusFields(entity.Quantity, entity.SalesOrderLineId, "tester"), Times.Once);

            _stockMovementRepoMock.Verify(r => r.AddAsync(It.Is<StockMovement>(sm =>
                sm.ProductId == entity.ProductId &&
                sm.QuantityChange == 0 - (entity.Quantity - oldDispatchLine.Quantity) &&
                sm.Reason == "Correction of dispatch line" &&
                sm.CrUserId == "tester"
            )), Times.Once);

            _repositoryMock.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldOnlyUpdateFields_WhenQuantityUnchanged()
        {
            // Arrange
            var entity = _entityFactory.CreateDispatchLine(); // same quantity

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync(_entityFactory.CreateDispatchLine());

            // Act
            await _service.UpdateAsync(entity, "tester", false);

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
            var entity = _entityFactory.CreateDispatchLine();

            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id))
                .ReturnsAsync((DispatchLine?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(entity, "tester", false));

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DispatchLine>()), Times.Never);
            _stockMovementRepoMock.Verify(r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
            _dispatchLineProcessorMock.Verify(p => p.SetSOStatusFields(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
