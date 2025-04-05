using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Placing order with {ItemCount} items.", createOrderDto.Items.Count);
            var order = await _orderService.PlaceOrderAsync(createOrderDto);
            return Ok(order);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto updateOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Updating order with ID {OrderId}", id);
            var order = await _orderService.UpdateOrderAsync(id, updateOrderDto);
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            _logger.LogInformation("Canceling order with ID {OrderId}", id);
            await _orderService.CancelOrderAsync(id);
            return NoContent();
        }

        [HttpGet("pending-orders")]
        public async Task<ActionResult<List<OrderDto>>> GetPendingOrders()
        {
            _logger.LogInformation("Fetching pending orders.");
            var orders = await _orderService.GetPendingOrdersAsync();
            return Ok(orders);
        }
    }
}