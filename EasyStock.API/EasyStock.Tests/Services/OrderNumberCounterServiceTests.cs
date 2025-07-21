using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EasyStock.API.Common;
using EasyStock.API.Repositories;
using EasyStock.API.Services;

namespace EasyStock.Tests.Services
{
    public class OrderNumberCounterServiceTests
    {
        private readonly Mock<IOrderNumberCounterRepository> _repositoryMock;
        private readonly IOrderNumberCounterService _service;

        public OrderNumberCounterServiceTests()
        {
            _repositoryMock = new Mock<IOrderNumberCounterRepository>();
            _service = new OrderNumberCounterService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldReturnCorrectFormat_ForPurchaseOrder()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync(OrderType.PurchaseOrder, It.IsAny<DateOnly>()))
                .ReturnsAsync(42);

            // Act
            var result = await _service.GenerateOrderNumberAsync(OrderType.PurchaseOrder);

            // Assert
            Assert.StartsWith("PO-", result);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), result);
            Assert.EndsWith("-00042", result);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldReturnCorrectFormat_ForSalesOrder()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync(OrderType.SalesOrder, It.IsAny<DateOnly>()))
                .ReturnsAsync(7);

            // Act
            var result = await _service.GenerateOrderNumberAsync(OrderType.SalesOrder);

            // Assert
            Assert.StartsWith("SO-", result);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), result);
            Assert.EndsWith("-00007", result);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldReturnCorrectFormat_ForReception()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync(OrderType.Reception, It.IsAny<DateOnly>()))
                .ReturnsAsync(99999);

            // Act
            var result = await _service.GenerateOrderNumberAsync(OrderType.Reception);

            // Assert
            Assert.StartsWith("RE-", result);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), result);
            Assert.EndsWith("-99999", result);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldReturnCorrectFormat_ForDispatch()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync(OrderType.Dispatch, It.IsAny<DateOnly>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GenerateOrderNumberAsync(OrderType.Dispatch);

            // Assert
            Assert.StartsWith("DI-", result);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), result);
            Assert.EndsWith("-00001", result);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldThrowException_ForUnsupportedOrderType()
        {
            // Arrange
            var unsupportedType = (OrderType)999;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.GenerateOrderNumberAsync(unsupportedType);
            });
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldPassCorrectParameters_ToRepository()
        {
            // Arrange
            OrderType expectedType = OrderType.SalesOrder;
            DateOnly? passedDate = null;
            _repositoryMock.Setup(r => r.GetNextOrderNumberAsync(expectedType, It.IsAny<DateOnly>()))
                .Callback<OrderType, DateOnly>((t, d) => passedDate = d)
                .ReturnsAsync(10);

            // Act
            await _service.GenerateOrderNumberAsync(expectedType);

            // Assert
            Assert.True(passedDate.HasValue);
            Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), passedDate.Value);
        }
    }
}
