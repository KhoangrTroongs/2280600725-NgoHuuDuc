﻿using System;
using System.Collections.Generic;

namespace NgoHuuDuc_2280600725.Models.ViewModels
{
    public class StatisticsViewModel
    {
        // Thống kê tổng quan
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        
        // Thống kê theo trạng thái đơn hàng
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnedOrders { get; set; }
        
        // Doanh thu theo trạng thái
        public decimal PendingRevenue { get; set; }
        public decimal ConfirmedRevenue { get; set; }
        public decimal ShippingRevenue { get; set; }
        public decimal DeliveredRevenue { get; set; }
        public decimal CancelledRevenue { get; set; }
        public decimal ReturnedRevenue { get; set; }
        
        // Thống kê sản phẩm bán chạy
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
        
        // Thống kê theo ngày
        public List<DailyStatisticsViewModel> DailyStatistics { get; set; } = new List<DailyStatisticsViewModel>();
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailyStatisticsViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
