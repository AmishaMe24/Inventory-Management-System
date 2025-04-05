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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            _logger.LogInformation("Fetching product with ID {ProductId}", id);
            var product = await _productService.GetProductAsync(id);
            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts()
        {
            _logger.LogInformation("Fetching all products.");
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateUpdateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Creating a new product with name {ProductName}", createProductDto.Name);
            var product = await _productService.CreateProductAsync(createProductDto);
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] CreateUpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Updating product with ID {ProductId}", id);
            await _productService.UpdateProductAsync(id, updateProductDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with ID {ProductId}", id);
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        [HttpGet("check-inventory/{productId}/{quantity}")]
        public async Task<ActionResult<bool>> CheckInventory(int productId, int quantity)
        {
            _logger.LogInformation("Checking inventory for product ID {ProductId}", productId);
            var hasEnoughStock = await _productService.CheckInventoryAsync(productId, quantity);
            return Ok(hasEnoughStock);
        }
    }
}