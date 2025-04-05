using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Repository
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetAllAsync();
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task<List<Order>> GetPendingOrdersAsync();
    }
}
