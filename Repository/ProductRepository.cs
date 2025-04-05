using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id) => await _context.Products.FindAsync(id);

        public async Task<Product> GetByNameAsync(string name) => await _context.Products.FirstOrDefaultAsync(p => p.Name == name);

        public async Task<List<Product>> GetAllAsync() => await _context.Products.ToListAsync();

        public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);

        public async Task UpdateAsync(Product product) => _context.Products.Update(product);

        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null) _context.Products.Remove(product);
        }
    }
}