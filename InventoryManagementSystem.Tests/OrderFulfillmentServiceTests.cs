using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagementSystem.Tests
{
    public class OrderFulfillmentServiceTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<OrderFulfillmentService>> _loggerMock;
        private readonly OrderFulfillmentService _orderFulfillmentService;

        public OrderFulfillmentServiceTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _orderServiceMock = new Mock<IOrderService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<OrderFulfillmentService>>();

            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_serviceScopeFactoryMock.Object);
            _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IOrderService)))
                .Returns(_orderServiceMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(INotificationService)))
                .Returns(_notificationServiceMock.Object);

            _orderFulfillmentService = new OrderFulfillmentService(_serviceProviderMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldProcessPendingOrders()
        {
            // Arrange
            var stoppingToken = new CancellationTokenSource();
            stoppingToken.CancelAfter(25000);

            var pendingOrders = new List<OrderDto>
            {
                new OrderDto { Id = 1, Status = OrderStatus.PendingFulfillment },
                new OrderDto { Id = 2, Status = OrderStatus.PendingFulfillment }
            };

            _orderServiceMock.Setup(x => x.GetPendingOrdersAsync()).ReturnsAsync(pendingOrders);
            _orderServiceMock.Setup(x => x.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<UpdateOrderDto>()))
                .ReturnsAsync(new OrderDto());
            _notificationServiceMock.Setup(x => x.SendNotificationAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _orderFulfillmentService.StartAsync(stoppingToken.Token);

            await Task.Delay(30000);

            await _orderFulfillmentService.StopAsync(stoppingToken.Token);
            // Assert
            _orderServiceMock.Verify(x => x.GetPendingOrdersAsync(), Times.AtLeastOnce);
            _orderServiceMock.Verify(x => x.UpdateOrderAsync(It.IsAny<int>(), It.IsAny<UpdateOrderDto>()), Times.AtLeastOnce);
            _notificationServiceMock.Verify(x => x.SendNotificationAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogErrorOnException()
        {
            // Arrange
            var stoppingToken = new CancellationTokenSource();
            stoppingToken.CancelAfter(2000);

            _orderServiceMock.Setup(x => x.GetPendingOrdersAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            await _orderFulfillmentService.StartAsync(stoppingToken.Token);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while processing orders.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}

