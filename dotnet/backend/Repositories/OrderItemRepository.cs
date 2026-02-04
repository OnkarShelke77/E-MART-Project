using EMart.Data;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly EMartDbContext _context;

        public OrderItemRepository(EMartDbContext context)
        {
            _context = context;
        }

        public async Task SaveAllAsync(IEnumerable<OrderItem> items)
        {
            _context.OrderItems.AddRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .ToListAsync();
        }
    }
}
