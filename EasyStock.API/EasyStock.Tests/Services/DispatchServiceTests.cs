using EasyStock.API.Common;
using EasyStock.API.Models;
using EasyStock.API.Repositories;
using EasyStock.API.Services;
using EasyStock.Tests.TestHelpers;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyStock.Tests.Services
{
    public class DispatchServiceTests
    {
        private readonly Mock<IDispatchRepository> _dispatchRepositoryMock;
        private readonly Mock<IRetryableTransactionService> _retryableTransactionServiceMock;
        private readonly Mock<IRepository<Dispatch>> _dispatchRepositoryGenericMock;
        private readonly Mock<IOrderNumberCounterService> _orderNumberCounterServiceMock;
        private readonly Mock<IRepository<SalesOrder>> _salesOrderRepositoryMock;
        private readonly Mock<IDispatchLineProcessor> _dispatchLineProcessorMock;

        private readonly EntityFactory _entityFactory;

        private readonly IDispatchService _service;

        public DispatchServiceTests()
        {
            _dispatchRepositoryMock = new Mock<IDispatchRepository>();
            _retryableTransactionServiceMock = new Mock<IRetryableTransactionService>();
            _dispatchRepositoryGenericMock = new Mock<IRepository<Dispatch>>();
            _orderNumberCounterServiceMock = new Mock<IOrderNumberCounterService>();
            _salesOrderRepositoryMock = new Mock<IRepository<SalesOrder>>();
            _dispatchLineProcessorMock = new Mock<IDispatchLineProcessor>();

            _entityFactory = new EntityFactory();

            _service = new DispatchService(
                _dispatchRepositoryMock.Object,
                _retryableTransactionServiceMock.Object,
                _dispatchRepositoryGenericMock.Object,
                _orderNumberCounterServiceMock.Object,
                _salesOrderRepositoryMock.Object,
                _dispatchLineProcessorMock.Object
            );
        }

        [Fact]
        public async Task AddAsync_Should_Add_Dispatch_And_DispatchLines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());

            var userName = "testUser";

            _orderNumberCounterServiceMock
                .Setup(s => s.GenerateOrderNumberAsync(OrderType.Dispatch))
                .ReturnsAsync("D123");

            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true))
                .Returns(Task.CompletedTask);

            _dispatchRepositoryGenericMock
                .Setup(r => r.AddAsync(dispatch))
                .Returns(Task.CompletedTask)
                .Verifiable();

            DispatchLine? capturedLine = null;
            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true))
                .Callback<DispatchLine, string, object, bool>((line, u, o, b) =>
                {
                    capturedLine = line;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(dispatch, userName, useTransaction: false);

            // Assert
            Assert.Equal("D123", dispatch.DispatchNumber);
            Assert.Equal(userName, dispatch.CrUserId);
            Assert.Equal(userName, dispatch.LcUserId);

            Assert.NotNull(capturedLine);
            Assert.Equal(1, capturedLine.LineNumber);

            _dispatchLineProcessorMock.Verify(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true), Times.Exactly(1));
            _dispatchRepositoryGenericMock.Verify(r => r.AddAsync(dispatch), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Set_LineNumbers_Correctly_When_MultipleLines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());

            var userName = "testUser";

            _orderNumberCounterServiceMock
                .Setup(s => s.GenerateOrderNumberAsync(OrderType.Dispatch))
                .ReturnsAsync("D123");

            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true))
                .Returns(Task.CompletedTask);

            _dispatchRepositoryGenericMock
                .Setup(r => r.AddAsync(dispatch))
                .Returns(Task.CompletedTask);

            var linesPassed = new List<DispatchLine>();

            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true))
                .Callback<DispatchLine, string, object, bool>((line, user, o, b) =>
                {
                    linesPassed.Add(line);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(dispatch, userName, useTransaction: false);

            // Assert
            Assert.Equal(2, linesPassed.Count);
            Assert.Equal(1, linesPassed[0].LineNumber);
            Assert.Equal(2, linesPassed[1].LineNumber);
        }

        [Fact]
        public async Task AddAsync_Should_Clear_Lines_From_Entity_Before_Adding()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines = new List<DispatchLine>
    {
        _entityFactory.CreateDispatchLine(),
        _entityFactory.CreateDispatchLine()
    };

            // Act
            await _service.AddAsync(dispatch, "testUser", useTransaction: false);

            // Assert
            // After AddAsync, Lines collection on original dispatch should be cleared (internal code clears it)
            Assert.Empty(dispatch.Lines);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_LineNumbers_Starting_From_1()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines = new List<DispatchLine>();

            var line1 = _entityFactory.CreateDispatchLine();
            line1.LineNumber = 5;
            var line2 = _entityFactory.CreateDispatchLine();
            line2.LineNumber = 10;

            dispatch.Lines.Add(line1);
            dispatch.Lines.Add(line2);

            var lineNumbers = new List<int>();
            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), It.IsAny<string>(), null, true))
                .Callback<DispatchLine, string, object?, bool>((line, user, _, __) =>
                {
                    lineNumbers.Add(line.LineNumber);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _service.AddAsync(dispatch, "testUser", useTransaction: false);

            // Assert
            Assert.Equal(new[] { 1, 2 }, lineNumbers);
        }

        [Fact]
        public async Task AddAsync_Should_Handle_Empty_Lines_List()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines = new List<DispatchLine>(); // empty list

            // Act
            await _service.AddAsync(dispatch, "testUser", useTransaction: false);

            // Assert
            // Repository AddAsync is called with the entity, no exceptions
            _dispatchRepositoryGenericMock.Verify(r => r.AddAsync(dispatch), Times.Once);
            _dispatchLineProcessorMock.Verify(p => p.AddAsync(It.IsAny<DispatchLine>(), It.IsAny<string>(), null, true), Times.Never);
        }

        [Fact]
        public async Task AddAsync_Should_Set_CrUserId_LcUserId_And_Dates_On_Dispatch()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();

            // Act
            await _service.AddAsync(dispatch, "testUser", useTransaction: false);

            // Assert
            Assert.Equal("testUser", dispatch.CrUserId);
            Assert.Equal("testUser", dispatch.LcUserId);
            Assert.True(dispatch.CrDate <= DateTime.UtcNow);
            Assert.True(dispatch.LcDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task AddFromSalesOrder_Should_Create_Dispatch_From_SalesOrder()
        {
            // Arrange
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Lines.Add(_entityFactory.CreateSalesOrderLine());

            var userName = "testUser";

            _salesOrderRepositoryMock
                .Setup(r => r.GetByIdAsync(salesOrder.Id))
                .ReturnsAsync(salesOrder);

            _orderNumberCounterServiceMock
                .Setup(s => s.GenerateOrderNumberAsync(OrderType.Dispatch))
                .ReturnsAsync("D123");

            _dispatchLineProcessorMock
                .Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true))
                .Returns(Task.CompletedTask);

            _dispatchRepositoryGenericMock
                .Setup(r => r.AddAsync(It.IsAny<Dispatch>()))
                .Returns(Task.CompletedTask);

            var futureDispatch = _entityFactory.CreateDispatch();
            futureDispatch.DispatchNumber = "D123";
            futureDispatch.CrUserId = userName;
            futureDispatch.LcUserId = userName;

            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => futureDispatch); 

            // Act
            var dispatch = await _service.AddFromSalesOrder(salesOrder.Id, userName, useTransaction: false);

            // Assert
            Assert.NotNull(dispatch);
            Assert.Equal("D123", dispatch.DispatchNumber);
            Assert.Equal(userName, dispatch.CrUserId);
            Assert.Equal(userName, dispatch.LcUserId);

            _dispatchLineProcessorMock.Verify(p => p.AddAsync(It.IsAny<DispatchLine>(), userName, null, true), Times.Exactly(salesOrder.Lines.Count));
            _dispatchRepositoryGenericMock.Verify(r => r.AddAsync(It.IsAny<Dispatch>()), Times.Once);
            _salesOrderRepositoryMock.Verify(r => r.GetByIdAsync(salesOrder.Id), Times.Once);
        }

        [Fact]
        public async Task AddFromSalesOrder_Should_Copy_Properties_From_SalesOrderLine_To_DispatchLine()
        {
            // Arrange
            var salesOrderLine = _entityFactory.CreateSalesOrderLine();
            salesOrderLine.Id = 1;
            salesOrderLine.LineNumber = 7;
            salesOrderLine.ProductId = 2;
            salesOrderLine.Quantity = 5;

            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Lines = new List<SalesOrderLine> { salesOrderLine };

            _salesOrderRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(salesOrder);

            DispatchLine capturedDispatchLine = null!;
            _dispatchLineProcessorMock.Setup(p => p.AddAsync(It.IsAny<DispatchLine>(), "testUser", null, true))
                .Callback<DispatchLine, string, object?, bool>((line, user, _, __) =>
                {
                    capturedDispatchLine = line;
                })
                .Returns(Task.CompletedTask);

            // Act
            var dispatch = await _service.AddFromSalesOrder(1, "testUser", useTransaction: false);

            // Assert
            Assert.NotNull(capturedDispatchLine);
            Assert.Equal(7, capturedDispatchLine.LineNumber);
            Assert.Equal(2, capturedDispatchLine.ProductId);
            Assert.Equal(5, capturedDispatchLine.Quantity);
            Assert.Equal(1, capturedDispatchLine.SalesOrderLineId);
            Assert.Equal(salesOrderLine, capturedDispatchLine.SalesOrderLine);
        }

        [Fact]
        public async Task AddFromSalesOrder_Should_Return_Dispatch_With_Lazy_Loaded_Lines()
        {
            // Arrange
            var salesOrder = _entityFactory.CreateSalesOrder();
            salesOrder.Lines = new List<SalesOrderLine> { _entityFactory.CreateSalesOrderLine() };

            _salesOrderRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(salesOrder);

            var disp = _entityFactory.CreateDispatch();
            disp.Id = 1;
            disp.Lines = new List<DispatchLine>() { _entityFactory.CreateDispatchLine() };

            _dispatchRepositoryGenericMock.SetupSequence(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(disp);

            // Act
            var dispatch = await _service.AddFromSalesOrder(1, "testUser", useTransaction: false);

            // Assert
            Assert.NotNull(dispatch);
            Assert.NotEmpty(dispatch.Lines);
        }

        [Fact]
        public async Task AddFromSalesOrder_Should_Throw_Exception_If_SalesOrder_Not_Found()
        {
            // Arrange
            _salesOrderRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((SalesOrder?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddFromSalesOrder(1, "testUser", useTransaction: false));
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Dispatch_And_All_Lines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine()); // Id=1
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine()); // Id=1 (but treat as two)
            var userName = "testUser";

            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(dispatch.Id))
                .ReturnsAsync(dispatch);

            _dispatchRepositoryGenericMock
                .Setup(r => r.DeleteAsync(dispatch.Id))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _dispatchLineProcessorMock
                .Setup(p => p.DeleteAsync(It.IsAny<int>(), userName))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(dispatch.Id, userName, useTransaction: false);

            // Assert
            _dispatchLineProcessorMock.Verify(p => p.DeleteAsync(It.IsAny<int>(), userName),
                Times.Exactly(dispatch.Lines.Count));
            _dispatchRepositoryGenericMock.Verify(r => r.DeleteAsync(dispatch.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_Dispatch_NotFound()
        {
            // Arrange
            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Dispatch?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.DeleteAsync(1, "testUser", useTransaction: false)
            );
        }

        [Fact]
        public async Task BlockAsync_Should_Set_BlDate_And_Call_Update_And_BlockLines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());
            var userName = "blockUser";

            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(dispatch.Id))
                .ReturnsAsync(dispatch);

            _dispatchRepositoryGenericMock
                .Setup(r => r.UpdateAsync(dispatch))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _dispatchLineProcessorMock
                .Setup(p => p.BlockAsync(It.IsAny<int>(), userName))
                .Returns(Task.CompletedTask);

            // Act
            await _service.BlockAsync(dispatch.Id, userName, useTransaction: false);

            // Assert
            Assert.Equal(userName, dispatch.BlUserId);
            Assert.True(dispatch.BlDate.HasValue);
            _dispatchRepositoryGenericMock.Verify(r => r.UpdateAsync(dispatch), Times.Once);
            _dispatchLineProcessorMock.Verify(p => p.BlockAsync(It.IsAny<int>(), userName),
                Times.Exactly(dispatch.Lines.Count));
        }

        [Fact]
        public async Task BlockAsync_Should_Throw_When_Dispatch_NotFound()
        {
            // Arrange
            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Dispatch?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.BlockAsync(1, "blockUser", useTransaction: false)
            );
        }

        [Fact]
        public async Task UnblockAsync_Should_Clear_BlockFields_And_Call_Update_And_UnblockLines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            dispatch.BlDate = DateTime.UtcNow;
            dispatch.BlUserId = "someone";
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());
            dispatch.Lines.Add(_entityFactory.CreateDispatchLine());
            var userName = "unblockUser";

            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(dispatch.Id))
                .ReturnsAsync(dispatch);

            _dispatchRepositoryGenericMock
                .Setup(r => r.UpdateAsync(dispatch))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _dispatchLineProcessorMock
                .Setup(p => p.UnblockAsync(It.IsAny<int>(), userName))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UnblockAsync(dispatch.Id, userName, useTransaction: false);

            // Assert
            Assert.Null(dispatch.BlDate);
            Assert.Null(dispatch.BlUserId);
            Assert.Equal(userName, dispatch.LcUserId);
            Assert.NotEqual(default, dispatch.LcDate);
            _dispatchRepositoryGenericMock.Verify(r => r.UpdateAsync(dispatch), Times.Once);
            _dispatchLineProcessorMock.Verify(p => p.UnblockAsync(It.IsAny<int>(), userName),
                Times.Exactly(dispatch.Lines.Count));
        }

        [Fact]
        public async Task UnblockAsync_Should_Throw_When_Dispatch_NotFound()
        {
            // Arrange
            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Dispatch?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.UnblockAsync(1, "unblockUser", useTransaction: false)
            );
        }

        [Fact]
        public async Task GetNextLineNumberAsync_Should_Return_MaxPlusOne_When_LinesExist()
        {
            // Arrange
            var line1 = _entityFactory.CreateDispatchLine();
            line1.LineNumber = 1;
            var line2 = _entityFactory.CreateDispatchLine();
            line2.LineNumber = 5;
            var line3 = _entityFactory.CreateDispatchLine();
            line3.LineNumber = 3;

            var dispatch = _entityFactory.CreateDispatch();
            dispatch.Lines.Add(line1);
            dispatch.Lines.Add(line2);
            dispatch.Lines.Add(line3);

            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(dispatch.Id))
                .ReturnsAsync(dispatch);

            // Act
            var nextLineNumber = await _service.GetNextLineNumberAsync(dispatch.Id);

            // Assert
            Assert.Equal(6, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_Should_Return_1_When_NoLines()
        {
            // Arrange
            var dispatch = _entityFactory.CreateDispatch();
            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(dispatch.Id))
                .ReturnsAsync(dispatch);

            // Act
            var nextLineNumber = await _service.GetNextLineNumberAsync(dispatch.Id);

            // Assert
            Assert.Equal(1, nextLineNumber);
        }

        [Fact]
        public async Task GetNextLineNumberAsync_Should_Throw_When_Dispatch_NotFound()
        {
            // Arrange
            _dispatchRepositoryGenericMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Dispatch?)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _service.GetNextLineNumberAsync(1)
            );
        }

    }
}
