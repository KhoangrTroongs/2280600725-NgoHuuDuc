using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Extensions;
using NgoHuuDuc_2280600725.Models;
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
                    return View(order);
                }

                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "ShoppingCart") });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    TempData["ErrorMessage"] = "Your cart could not be found. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                if (cart.Items.Count == 0)
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items to your cart before checkout.";
                    return RedirectToAction("Index", "Home");
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "ShoppingCart") });
                }

                order.UserId = user.Id;
                order.OrderDate = DateTime.UtcNow;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

                _context.Orders.Add(order);

                // Remove cart items and cart after order is placed
                _context.CartItems.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();

                // Redirect to order completed page
                return View("OrderCompleted", order.Id);
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        public async Task<IActionResult> GetOrderSummary()
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
    }
}
