using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Extensions;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using System;
using System.Collections.Generic;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return View(new List<CartItem>());
                }

                return View(cart.Items);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShoppingCart Index: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your cart.";
                return View(new List<CartItem>());
            }
        }

        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = User.Identity?.Name;
                Console.WriteLine($"Checkout attempted by user: {userId}"); // Debug logging

                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return RedirectToAction("Index", "Home");
                }

                var order = new Order();
                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Checkout: {ex.Message}"); // Debug logging
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine($"Validation error: {error.ErrorMessage}");
                        }
                    }
                    return View(order);
                }

                // Lấy thông tin người dùng
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items to your cart before checkout.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin người dùng
                var user = await _userManager.FindByNameAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không thể tìm thấy thông tin người dùng.";
                    return RedirectToAction("Login", "Account");
                }

                // Tạo đơn hàng mới
                var newOrder = new Order
                {
                    UserId = user.Id, // Sử dụng ID thực của người dùng
                    OrderDate = DateTime.Now,
                    TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity),
                    Status = OrderStatus.Pending,
                    ShippingAddress = order.ShippingAddress,
                    Notes = order.Notes
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                // Tạo chi tiết đơn hàng
                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Kiểm tra số lượng
                        if (product.Quantity < item.Quantity)
                        {
                            TempData["ErrorMessage"] = $"Sản phẩm '{item.ProductName}' chỉ còn {product.Quantity} sản phẩm trong kho.";
                            _context.Orders.Remove(newOrder);
                            await _context.SaveChangesAsync();
                            return View(order);
                        }

                        // Tạo chi tiết đơn hàng
                        var orderDetail = new OrderDetail
                        {
                            OrderId = newOrder.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };

                        _context.Add(orderDetail);

                        // Cập nhật số lượng sản phẩm
                        product.Quantity -= item.Quantity;
                        _context.Update(product);
                    }
                }

                // Xóa giỏ hàng
                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                return RedirectToAction("OrderCompleted", new { id = newOrder.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Checkout: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult OrderCompleted(int id)
        {
            return View(id);
        }

        public async Task<IActionResult> GetOrderSummary()
        {
            // Kiểm tra nếu đây là một yêu cầu AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return PartialView("_OrderSummaryPartial", new List<CartItem>());
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                {
                    return PartialView("_OrderSummaryPartial", new List<CartItem>());
                }

                return PartialView("_OrderSummaryPartial", cart.Items);
            }
            else
            {
                // Nếu đây là một yêu cầu trực tiếp từ trình duyệt, chuyển hướng đến trang MyOrders
                return RedirectToAction(nameof(MyOrders));
            }
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Where(o => o.UserId == user.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                if (orders == null || orders.Count == 0)
                {
                    ViewBag.Message = "You have no orders yet.";
                    return View(new List<Order>());
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MyOrders: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your orders.";
                return View(new List<Order>());
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found or you don't have permission to view it.";
                    return RedirectToAction(nameof(MyOrders));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OrderDetails: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading your order details.";
                return RedirectToAction(nameof(MyOrders));
            }
        }
    }
}
