using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Claims;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> GetCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
                }

                var cart = await _cartService.GetCartAsync(userId);
                if (cart == null)
                {
                    // Return empty cart instead of 404
                    return Ok(ResponseDTO<CartDTO>.Success(new CartDTO
                    {
                        UserId = userId,
                        Items = new List<CartItemDTO>(),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }));
                }

                return Ok(ResponseDTO<CartDTO>.Success(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, ResponseDTO<CartDTO>.Fail("An error occurred while retrieving the cart."));
            }
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> AddToCart(AddToCartDTO addToCartDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<CartDTO>.Fail("Invalid cart data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var cart = await _cartService.AddToCartAsync(userId, addToCartDto);
                return Ok(ResponseDTO<CartDTO>.Success(cart));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error adding to cart");
                return BadRequest(ResponseDTO<CartDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                return StatusCode(500, ResponseDTO<CartDTO>.Fail("An error occurred while adding to the cart."));
            }
        }

        // PUT: api/Cart
        [HttpPut]
        public async Task<ActionResult<ResponseDTO<CartDTO>>> UpdateCartItem(UpdateCartItemDTO updateCartItemDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<CartDTO>.Fail("User not authenticated."));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<CartDTO>.Fail("Invalid cart data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var cart = await _cartService.UpdateCartItemAsync(userId, updateCartItemDto);
                if (cart == null)
                {
                    return NotFound(ResponseDTO<CartDTO>.Fail("Cart not found."));
                }

                return Ok(ResponseDTO<CartDTO>.Success(cart));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating cart item");
                return BadRequest(ResponseDTO<CartDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, ResponseDTO<CartDTO>.Fail("An error occurred while updating the cart item."));
            }
        }

        // DELETE: api/Cart/5
        [HttpDelete("{cartItemId}")]
        public async Task<ActionResult<ResponseDTO<bool>>> RemoveCartItem(int cartItemId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<bool>.Fail("User not authenticated."));
                }

                var result = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Cart item not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Cart item removed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while removing the cart item."));
            }
        }

        // DELETE: api/Cart
        [HttpDelete]
        public async Task<ActionResult<ResponseDTO<bool>>> ClearCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<bool>.Fail("User not authenticated."));
                }

                var result = await _cartService.ClearCartAsync(userId);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Cart not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Cart cleared successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while clearing the cart."));
            }
        }
    }
}
