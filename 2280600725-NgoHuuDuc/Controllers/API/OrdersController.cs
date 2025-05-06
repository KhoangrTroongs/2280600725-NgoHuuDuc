using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NgoHuuDuc_2280600725.DTOs;
using NgoHuuDuc_2280600725.Services.Interfaces;
using System.Security.Claims;

namespace NgoHuuDuc_2280600725.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: api/Orders
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<IEnumerable<OrderDTO>>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(ResponseDTO<IEnumerable<OrderDTO>>.Success(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return StatusCode(500, ResponseDTO<IEnumerable<OrderDTO>>.Fail("An error occurred while retrieving orders."));
            }
        }

        // GET: api/Orders/my-orders
        [HttpGet("my-orders")]
        public async Task<ActionResult<ResponseDTO<IEnumerable<OrderDTO>>>> GetMyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<IEnumerable<OrderDTO>>.Fail("User not authenticated."));
                }

                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(ResponseDTO<IEnumerable<OrderDTO>>.Success(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders");
                return StatusCode(500, ResponseDTO<IEnumerable<OrderDTO>>.Fail("An error occurred while retrieving orders."));
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDTO<OrderDTO>>> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(ResponseDTO<OrderDTO>.Fail("Order not found."));
                }

                // Check if user is authorized to view this order
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Administrator");
                
                if (order.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                return Ok(ResponseDTO<OrderDTO>.Success(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {Id}", id);
                return StatusCode(500, ResponseDTO<OrderDTO>.Fail("An error occurred while retrieving the order."));
            }
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<OrderDTO>>> CreateOrder(CreateOrderDTO orderDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseDTO<OrderDTO>.Fail("User not authenticated."));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<OrderDTO>.Fail("Invalid order data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var order = await _orderService.CreateOrderAsync(userId, orderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ResponseDTO<OrderDTO>.Success(order));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating order");
                return BadRequest(ResponseDTO<OrderDTO>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, ResponseDTO<OrderDTO>.Fail("An error occurred while creating the order."));
            }
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<OrderDTO>>> UpdateOrderStatus(int id, UpdateOrderStatusDTO updateOrderStatusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseDTO<OrderDTO>.Fail("Invalid order status data.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                }

                var order = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto);
                if (order == null)
                {
                    return NotFound(ResponseDTO<OrderDTO>.Fail("Order not found."));
                }

                return Ok(ResponseDTO<OrderDTO>.Success(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status {Id}", id);
                return StatusCode(500, ResponseDTO<OrderDTO>.Fail("An error occurred while updating the order status."));
            }
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ResponseDTO<bool>>> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                {
                    return NotFound(ResponseDTO<bool>.Fail("Order not found."));
                }

                return Ok(ResponseDTO<bool>.Success(true, "Order deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {Id}", id);
                return StatusCode(500, ResponseDTO<bool>.Fail("An error occurred while deleting the order."));
            }
        }
    }
}
