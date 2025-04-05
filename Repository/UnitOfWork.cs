using InventoryManagementSystem.Data;
using InventoryManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
        }

        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }

        public async Task<int> SaveChangesAsync()
        {
            int retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    return await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, $"Concurrency conflict detected. Retry attempt {retryCount} of {maxRetries}.");

                    foreach (var entry in ex.Entries)
                    {
                        var proposedValues = entry.CurrentValues;
                        var databaseValues = await entry.GetDatabaseValuesAsync();

                        if (databaseValues == null)
                        {
                            _logger.LogWarning("The entity being updated no longer exists in the database.");
                            throw new ConflictException("The record you are trying to update has been deleted by another user.");
                        }

                        entry.OriginalValues.SetValues(databaseValues);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database update error occurred while saving changes.");
                    throw new BadRequestException("An error occurred while updating the database. Please check your input.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while saving changes.");
                    throw new CustomException(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "An unexpected error occurred. Please try again later."
                    );
                }
            }

            _logger.LogError("Max retry attempts reached for concurrency conflict.");
            throw new ConflictException("Unable to save changes due to a concurrency conflict. Please reload the data and try again.");
        }

        public void Dispose() => _context.Dispose();
    }
}