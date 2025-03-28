using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace NgoHuuDuc_2280600725.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId)
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
                var selectedCategory = await _context.Categories.FindAsync(categoryId.Value);
                ViewBag.SelectedCategoryName = selectedCategory?.Name ?? "Unknown Category";
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await products.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart?.Items ?? new List<CartItem>());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = User.Identity.Name;
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                };
                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                cartCount = cart.Items.Sum(x => x.Quantity)
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    cart.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        cartCount = cart.Items.Sum(x => x.Quantity)
                    });
                }
            }

            return Json(new { success = false });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var count = cart?.Items.Sum(x => x.Quantity) ?? 0;
            return Json(new { count });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1) return Json(new { success = false });

            var userId = User.Identity.Name;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    cart.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    return Json(new { 
                        success = true, 
                        cartCount = cart.Items.Sum(x => x.Quantity)
                    });
                }
            }

            return Json(new { success = false });
        }
    }
}
