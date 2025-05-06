using System.ComponentModel.DataAnnotations;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;

namespace NgoHuuDuc_2280600725.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? UserName { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string ShippingAddress { get; set; } = "";

        public string? Notes { get; set; }

        public OrderStatus Status { get; set; }

        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }

    public class OrderDetailDTO
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = "";

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? ProductImageUrl { get; set; }
    }

    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        public string ShippingAddress { get; set; } = "";

        public string? Notes { get; set; }
    }

    public class UpdateOrderStatusDTO
    {
        [Required(ErrorMessage = "Trạng thái đơn hàng không được để trống")]
        public OrderStatus Status { get; set; }
    }
}
