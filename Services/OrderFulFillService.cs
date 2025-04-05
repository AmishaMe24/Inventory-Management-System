using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services
{
    public class OrderFulfillmentService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OrderFulfillmentService> _logger;

        public OrderFulfillmentService(IServiceProvider services, ILogger<OrderFulfillmentService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var pendingOrders = await orderService.GetPendingOrdersAsync();
                        _logger.LogInformation("Found {Count} pending orders.", pendingOrders.Count);

                        foreach (var order in pendingOrders)
                        {
                            _logger.LogInformation("Processing order with ID {OrderId}", order.Id);

                            await Task.Delay(5000, stoppingToken);

                            var updateOrderDto = new UpdateOrderDto
                            {
                                Status = OrderStatus.Fulfilled
                            };

                            await orderService.UpdateOrderAsync(order.Id, updateOrderDto);

                            await notificationService.SendNotificationAsync($"Order {order.Id} has been fulfilled.");
                            _logger.LogInformation("Order with ID {OrderId} fulfilled successfully.", order.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing orders.");
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}