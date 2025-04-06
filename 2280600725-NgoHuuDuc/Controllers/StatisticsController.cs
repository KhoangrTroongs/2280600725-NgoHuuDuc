﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Models.Enums;
using NgoHuuDuc_2280600725.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NgoHuuDuc_2280600725.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Statistics
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Mặc định là thống kê tháng hiện tại
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }

            // Đảm bảo endDate là cuối ngày
            endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            // Lấy tất cả đơn hàng trong khoảng thời gian
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            // Thống kê tổng quan
            var statistics = new StatisticsViewModel
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalPrice),
                TotalProducts = await _context.Products.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                
                // Thống kê theo trạng thái đơn hàng
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                ConfirmedOrders = orders.Count(o => o.Status == OrderStatus.Confirmed),
                ShippingOrders = orders.Count(o => o.Status == OrderStatus.Shipping),
                DeliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
                ReturnedOrders = orders.Count(o => o.Status == OrderStatus.Returned),
                
                // Doanh thu theo trạng thái
                PendingRevenue = orders.Where(o => o.Status == OrderStatus.Pending).Sum(o => o.TotalPrice),
                ConfirmedRevenue = orders.Where(o => o.Status == OrderStatus.Confirmed).Sum(o => o.TotalPrice),
                ShippingRevenue = orders.Where(o => o.Status == OrderStatus.Shipping).Sum(o => o.TotalPrice),
                DeliveredRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice),
                CancelledRevenue = orders.Where(o => o.Status == OrderStatus.Cancelled).Sum(o => o.TotalPrice),
                ReturnedRevenue = orders.Where(o => o.Status == OrderStatus.Returned).Sum(o => o.TotalPrice),
                
                // Thống kê sản phẩm bán chạy
                TopProducts = orders
                    .SelectMany(o => o.OrderDetails)
                    .GroupBy(od => new { od.ProductId, Name = od.Product.Name, ImageUrl = od.Product.ImageUrl })
                    .Select(g => new TopProductViewModel
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        Quantity = g.Sum(od => od.Quantity),
                        Revenue = g.Sum(od => od.Price * od.Quantity)
                    })
                    .OrderByDescending(p => p.Quantity)
                    .Take(10)
                    .ToList(),
                
                // Thống kê theo ngày
                DailyStatistics = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyStatisticsViewModel
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return View(statistics);
        }
    }
}
