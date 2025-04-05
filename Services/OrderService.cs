using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Exceptions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services
{
    public interface IOrderService
    {
        Task<OrderDto> PlaceOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto);
        Task CancelOrderAsync(int id);
        Task<List<OrderDto>> GetPendingOrdersAsync();
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OrderDto> PlaceOrderAsync(CreateOrderDto createOrderDto)
        {
            _logger.LogInformation("Placing order with {ItemCount} items.", createOrderDto.Items?.Count ?? 0);

            var order = _mapper.Map<Order>(createOrderDto) ?? new Order();

            order.Items = order.Items ?? new List<OrderItem>();

            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.PendingFulfillment;
            order.Version = Guid.NewGuid().ToByteArray();

            foreach (var item in order.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", item.ProductId);
                    throw new NotFoundException($"Product with ID {item.ProductId} not found.");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for product ID {ProductId}. " +
                                       "Available: {StockQty}, Requested: {Qty}",
                                       item.ProductId, product.StockQuantity, item.Quantity);
                    throw new BadRequestException($"Insufficient stock for product {product.Name}.");
                }

                product.StockQuantity -= item.Quantity;
                product.Version = Guid.NewGuid().ToByteArray();
            }

            await _unitOfWork.Orders.AddAsync(order);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while placing the order.");
                throw new ConflictException("A concurrency conflict occurred. Please try again.");
            }

            _logger.LogInformation("Successfully placed order with ID {OrderId}", order.Id);
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto)
        {
            _logger.LogInformation("Updating order with ID {OrderId}", id);

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
                throw new NotFoundException($"Order with ID {id} not found.");
            }

            _mapper.Map(updateOrderDto, order);
            order.Version = Guid.NewGuid().ToByteArray();

            await _unitOfWork.Orders.UpdateAsync(order);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while updating the order.");
                throw new ConflictException("A concurrency conflict occurred. Please reload the data and try again.");
            }

            _logger.LogInformation("Successfully updated order with ID {OrderId}", id);
            return _mapper.Map<OrderDto>(order);
        }

        public async Task CancelOrderAsync(int id)
        {
            _logger.LogInformation("Canceling order with ID {OrderId}", id);

            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", id);
                throw new NotFoundException($"Order with ID {id} not found.");
            }

            foreach (var item in order.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.Version = Guid.NewGuid().ToByteArray();
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.Version = Guid.NewGuid().ToByteArray();

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while canceling the order.");
                throw new ConflictException("A concurrency conflict occurred. Please reload the data and try again.");
            }

            _logger.LogInformation("Successfully canceled order with ID {OrderId}", id);
        }

        public async Task<List<OrderDto>> GetPendingOrdersAsync()
        {
            _logger.LogInformation("Fetching pending orders.");

            var orders = await _unitOfWork.Orders.GetPendingOrdersAsync();
            _logger.LogInformation("Found {Count} pending orders.", orders.Count);

            return _mapper.Map<List<OrderDto>>(orders);
        }
    }
}
