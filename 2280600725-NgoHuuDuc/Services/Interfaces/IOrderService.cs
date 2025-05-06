using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Models;

namespace NgoHuuDuc_2280600725.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetUserOrdersAsync(string userId);
        Task<OrderDTO?> GetOrderByIdAsync(int id);
        Task<OrderDTO> CreateOrderAsync(string userId, CreateOrderDTO orderDto);
        Task<OrderDTO?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDTO updateOrderStatusDto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
