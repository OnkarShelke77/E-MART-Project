using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMart.Data;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/order-items")] // Matches Java @RequestMapping("/api/order-items")
    public class OrderItemController : ControllerBase
    {
        private readonly EMartDbContext _context;

        public OrderItemController(EMartDbContext context)
        {
            _context = context;
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<OrderItem>>> GetItemsByOrder(int orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }
    }
}
