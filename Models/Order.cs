using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    #region enum

    public enum OrderStatus
    {
        PendingFulfillment,
        Fulfilled,
        Cancelled
    }

    #endregion

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public byte[] Version { get; set; } = Array.Empty<byte>();
    }

    #region DTO
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class CreateOrderDto
    {
        [Required(ErrorMessage = "At least one item is required.")]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class UpdateOrderDto
    {
        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Invalid order status.")]
        public OrderStatus Status { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    #endregion
}
