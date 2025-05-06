using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Services.Interfaces;

namespace NgoHuuDuc_2280600725.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ApplicationDbContext context,
            ILogger<CartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CartDTO?> GetCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart != null ? MapToCartDTO(cart) : null;
        }

        public async Task<CartDTO> AddToCartAsync(string userId, AddToCartDTO addToCartDto)
        {
            // Check if product exists and has enough quantity
            var product = await _context.Products.FindAsync(addToCartDto.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            if (product.Quantity < addToCartDto.Quantity)
            {
                throw new InvalidOperationException("Số lượng sản phẩm không đủ.");
            }

            // Get or create cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if product already in cart
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);
            if (cartItem != null)
            {
                // Update quantity
                cartItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                // Add new item
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = addToCartDto.Quantity,
                    ImageUrl = product.ImageUrl
                };
                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Reload cart with products
            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Id == cart.Id);

            return MapToCartDTO(cart);
        }

        public async Task<CartDTO?> UpdateCartItemAsync(string userId, UpdateCartItemDTO updateCartItemDto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return null;
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == updateCartItemDto.CartItemId);
            if (cartItem == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại trong giỏ hàng.");
            }

            // Check if product has enough quantity
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Sản phẩm không tồn tại.");
            }

            if (product.Quantity < updateCartItemDto.Quantity)
            {
                throw new InvalidOperationException("Số lượng sản phẩm không đủ.");
            }

            if (updateCartItemDto.Quantity <= 0)
            {
                // Remove item if quantity is 0 or negative
                cart.Items.Remove(cartItem);
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Update quantity
                cartItem.Quantity = updateCartItemDto.Quantity;
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return MapToCartDTO(cart);
        }

        public async Task<bool> RemoveCartItemAsync(string userId, int cartItemId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return false;
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
            {
                return false;
            }

            cart.Items.Remove(cartItem);
            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return false;
            }

            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        private CartDTO MapToCartDTO(Cart cart)
        {
            return new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new CartItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList(),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }
    }
}
