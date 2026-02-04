using EMart.Data;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EMartDbContext _context;

        public OrderRepository(EMartDbContext context)
        {
            _context = context;
        }

        public async Task<Ordermaster> SaveAsync(Ordermaster order)
        {
            if (order.Id == 0)
            {
                _context.Ordermasters.Add(order);
            }
            else
            {
                _context.Ordermasters.Update(order);
            }
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<IEnumerable<Ordermaster>> GetAllAsync()
        {
            return await _context.Ordermasters
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ToListAsync();
        }

        public async Task<Ordermaster?> GetByIdAsync(int id)
        {
            return await _context.Ordermasters
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Ordermaster>> GetByUserIdAsync(int userId)
        {
            return await _context.Ordermasters
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .ToListAsync();
        }
    }
}
