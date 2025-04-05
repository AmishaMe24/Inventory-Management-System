using AutoMapper;
using global::InventoryManagementSystem.Models;
using global::InventoryManagementSystem.Repository;
using global::InventoryManagementSystem.Services;
using InventoryManagementSystem.Exceptions;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository;
using InventoryManagementSystem.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace InventoryManagementSystem.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _mapperMock = new Mock<IMapper>();
            _orderService = new OrderService(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldPlaceOrderSuccessfully()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 }
                }
            };
            var order = new Order { Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2 } } };
            var product = new Product { Id = 1, StockQuantity = 10 };

            _mapperMock.Setup(m => m.Map<Order>(createOrderDto)).Returns(order);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.Orders.AddAsync(order)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.PlaceOrderAsync(createOrderDto);

            // Assert
            _unitOfWorkMock.Verify(u => u.Products.GetByIdAsync(1), Times.Once);
            _unitOfWorkMock.Verify(u => u.Orders.AddAsync(order), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<OrderDto>(order), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderAsync_ShouldUpdateOrderSuccessfully()
        {
            // Arrange
            var updateOrderDto = new UpdateOrderDto { Status = OrderStatus.Fulfilled };
            var order = new Order { Id = 1, Status = OrderStatus.PendingFulfillment };

            _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(1)).ReturnsAsync(order);
            _mapperMock.Setup(m => m.Map(updateOrderDto, order)).Verifiable();
            _unitOfWorkMock.Setup(u => u.Orders.UpdateAsync(order)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.UpdateOrderAsync(1, updateOrderDto);

            // Assert
            _unitOfWorkMock.Verify(u => u.Orders.GetByIdAsync(1), Times.Once);
            _mapperMock.Verify(m => m.Map(updateOrderDto, order), Times.Once);
            _unitOfWorkMock.Verify(u => u.Orders.UpdateAsync(order), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<OrderDto>(order), Times.Once);
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldCancelOrderSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2 } },
                Status = OrderStatus.PendingFulfillment
            };
            var product = new Product { Id = 1, StockQuantity = 10 };

            _unitOfWorkMock.Setup(u => u.Orders.GetByIdAsync(1)).ReturnsAsync(order);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _orderService.CancelOrderAsync(1);

            // Assert
            _unitOfWorkMock.Verify(u => u.Orders.GetByIdAsync(1), Times.Once);
            _unitOfWorkMock.Verify(u => u.Products.GetByIdAsync(1), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetPendingOrdersAsync_ShouldReturnPendingOrders()
        {
            // Arrange
            var orders = new List<Order> { new Order { Id = 1, Status = OrderStatus.PendingFulfillment } };

            _unitOfWorkMock.Setup(u => u.Orders.GetPendingOrdersAsync()).ReturnsAsync(orders);
            _mapperMock.Setup(m => m.Map<List<OrderDto>>(orders)).Returns(new List<OrderDto> { new OrderDto { Id = 1 } });

            // Act
            var result = await _orderService.GetPendingOrdersAsync();

            // Assert
            _unitOfWorkMock.Verify(u => u.Orders.GetPendingOrdersAsync(), Times.Once);
            _mapperMock.Verify(m => m.Map<List<OrderDto>>(orders), Times.Once);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldHandleMultipleSimultaneousOrders()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 }
                }
            };
            var order = new Order { Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2 } } };
            var product = new Product { Id = 1, StockQuantity = 100 };

            _mapperMock.Setup(m => m.Map<Order>(createOrderDto)).Returns(order);
            _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1)).ReturnsAsync(product);
            _unitOfWorkMock.Setup(u => u.Orders.AddAsync(order)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var tasks = new List<Task<OrderDto>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_orderService.PlaceOrderAsync(createOrderDto));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            _unitOfWorkMock.Verify(u => u.Products.GetByIdAsync(1), Times.Exactly(10));
            _unitOfWorkMock.Verify(u => u.Orders.AddAsync(order), Times.Exactly(10));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(10));
            _mapperMock.Verify(m => m.Map<OrderDto>(order), Times.Exactly(10));
        }
    }
}
