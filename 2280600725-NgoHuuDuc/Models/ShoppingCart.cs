using System;
using System.Collections.Generic;
using System.Linq;

namespace NgoHuuDuc_2280600725.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        public decimal GetTotal()
        {
            return Items.Sum(item => item.Price * item.Quantity);
        }
    }
}
