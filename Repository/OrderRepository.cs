using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace InventoryManagementSystem.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(int id) => await _context.Orders.FindAsync(id);

        public async Task<List<Order>> GetAllAsync() => await _context.Orders.ToListAsync();

        public async Task AddAsync(Order order) => await _context.Orders.AddAsync(order);

        public async Task UpdateAsync(Order order) => _context.Orders.Update(order);

        public async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order != null) _context.Orders.Remove(order);
        }

        public async Task<List<Order>> GetPendingOrdersAsync() =>
            await _context.Orders.Where(o => o.Status == OrderStatus.PendingFulfillment).ToListAsync();
    }
}
