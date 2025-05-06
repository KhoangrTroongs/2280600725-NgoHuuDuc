using NgoHuuDuc_2280600725.DTOs;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO?> GetCartAsync(string userId);
        Task<CartDTO> AddToCartAsync(string userId, AddToCartDTO addToCartDto);
        Task<CartDTO?> UpdateCartItemAsync(string userId, UpdateCartItemDTO updateCartItemDto);
        Task<bool> RemoveCartItemAsync(string userId, int cartItemId);
        Task<bool> ClearCartAsync(string userId);
    }
}
