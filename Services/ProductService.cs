using System.Threading.Tasks;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using InventoryManagementSystem.Exceptions;
using AutoMapper;

namespace InventoryManagementSystem.Services
{
    #region IProductService interface
    public interface IProductService
    {
        Task<ProductDto> GetProductAsync(int id);
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateUpdateProductDto createProductDto);
        Task UpdateProductAsync(int id, CreateUpdateProductDto updateProductDto);
        Task DeleteProductAsync(int id);
        Task<bool> CheckInventoryAsync(int productId, int quantity);
    }

    #endregion

    #region ProductService Class
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetProductAsync(int id)
        {
            _logger.LogInformation("Retrieving product with ID {ProductId}", id);
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            _logger.LogInformation("Successfully retrieved product with ID {ProductId}", id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            _logger.LogInformation("Retrieving all products.");
            var products = await _unitOfWork.Products.GetAllAsync();
            _logger.LogInformation("Successfully retrieved {Count} products.", products.Count);
            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateUpdateProductDto createProductDto)
        {
            _logger.LogInformation("Creating a new product with name {ProductName}", createProductDto.Name);

            var existingProduct = await _unitOfWork.Products.GetByNameAsync(createProductDto.Name);
            if (existingProduct != null)
            {
                _logger.LogWarning("Product with name {ProductName} already exists.", createProductDto.Name);
                throw new ConflictException("Product with the same name already exists.");
            }

            var product = _mapper.Map<Product>(createProductDto);
            product.Version = Guid.NewGuid().ToByteArray();

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created product with ID {ProductId}", product.Id);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task UpdateProductAsync(int id, CreateUpdateProductDto updateProductDto)
        {
            _logger.LogInformation("Updating product with ID {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            _mapper.Map(updateProductDto, product);
            product.Version = Guid.NewGuid().ToByteArray();

            await _unitOfWork.Products.UpdateAsync(product);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while updating the product.");
                throw;
            }
            _logger.LogInformation("Successfully updated product with ID {ProductId}", id);
        }

        public async Task DeleteProductAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
                throw new NotFoundException($"Product with ID {id} not found.");
            }

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted product with ID {ProductId}", id);
        }

        public async Task<bool> CheckInventoryAsync(int productId, int quantity)
        {
            _logger.LogInformation("Checking inventory for product ID {ProductId}", productId);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            bool hasEnoughStock = product.StockQuantity >= quantity;
            _logger.LogInformation("Inventory check for product ID {ProductId}: {HasEnoughStock}", productId, hasEnoughStock);

            return hasEnoughStock;
        }
    }
    #endregion
}
