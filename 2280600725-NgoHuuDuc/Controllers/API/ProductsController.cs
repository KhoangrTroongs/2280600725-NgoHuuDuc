using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Helpers;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<IEnumerable<ProductDTO>>>> GetProducts([FromQuery] int? categoryId)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.GetProductsByCategoryAsync(categoryId)
                    : await _productService.GetProductsByCategoryAsync(categoryId, false);
                return Ok(ResponseDTO<IEnumerable<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, ResponseDTO<IEnumerable<ProductDTO>>.Fail("An error occurred while retrieving products."));
            }
        }

        // GET: api/Products/paged?categoryId=1&pageIndex=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult<ResponseDTO<PaginatedList<ProductDTO>>>> GetPagedProducts(
            [FromQuery] int? categoryId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize)
                    : await _productService.GetProductsByCategoryAsync(categoryId, pageIndex, pageSize, false);
                return Ok(ResponseDTO<PaginatedList<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged products");
                return StatusCode(500, ResponseDTO<PaginatedList<ProductDTO>>.Fail("An error occurred while retrieving products."));
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }

                // Nếu sản phẩm bị ẩn và người dùng không phải admin, trả về NotFound
                if (product.IsHidden && !User.IsInRole("Administrator"))
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }



                return Ok(ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {Id}", id);
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while retrieving the product."));
            }
        }

        // GET: api/Products/search?keyword=shirt&pageIndex=1&pageSize=10
        [HttpGet("search")]
        public async Task<ActionResult<ResponseDTO<PaginatedList<ProductDTO>>>> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // Nếu là admin, hiển thị tất cả sản phẩm, ngược lại chỉ hiển thị sản phẩm không bị ẩn
                var products = User.IsInRole("Administrator")
                    ? await _productService.SearchProductsAsync(keyword, pageIndex, pageSize)
                    : await _productService.SearchProductsAsync(keyword, pageIndex, pageSize, false);
                return Ok(ResponseDTO<PaginatedList<ProductDTO>>.Success(products));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with keyword {Keyword}", keyword);
                return StatusCode(500, ResponseDTO<PaginatedList<ProductDTO>>.Fail("An error occurred while searching for products."));
            }
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> CreateProduct([FromForm] CreateProductDTO productDto, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<ProductDTO>.Fail("Invalid product data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var product = await _productService.AddProductAsync(productDto, image);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while creating the product."));
            }
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<ProductDTO>>> UpdateProduct(int id, [FromForm] UpdateProductDTO productDto, IFormFile? image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<ProductDTO>.Fail("Invalid product data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var product = await _productService.UpdateProductAsync(id, productDto, image);
                if (product == null)
                {
                    return NotFound(ResponseDTO<ProductDTO>.Fail("Product not found."));
                }

                return Ok(ResponseDTO<ProductDTO>.Success(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {Id}", id);
                return StatusCode(500, ResponseDTO<ProductDTO>.Fail("An error occurred while updating the product."));
            }
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Product not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Product deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {Id}", id);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while deleting the product."));
            }
        }
    }
}
