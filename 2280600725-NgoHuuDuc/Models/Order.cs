using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NgoHuuDuc_2280600725.Models.Enums;

namespace NgoHuuDuc_2280600725.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = "";

        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
