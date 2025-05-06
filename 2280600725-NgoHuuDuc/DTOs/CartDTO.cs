namespace NgoHuuDuc_2280600725.DTOs
{
    public class CartDTO
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = "";
        
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
        
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }

    public class CartItemDTO
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        public string ProductName { get; set; } = "";
        
        public decimal Price { get; set; }
        
        public int Quantity { get; set; }
        
        public string ImageUrl { get; set; } = "";
    }

    public class AddToCartDTO
    {
        public int ProductId { get; set; }
        
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemDTO
    {
        public int CartItemId { get; set; }
        
        public int Quantity { get; set; }
    }
}
