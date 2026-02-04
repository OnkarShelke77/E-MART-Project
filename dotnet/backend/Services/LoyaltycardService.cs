using EMart.Data;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Services
{
    public class LoyaltycardService : ILoyaltycardService
    {
        private readonly EMartDbContext _context;

        public LoyaltycardService(EMartDbContext context)
        {
            _context = context;
        }

        public async Task<Loyaltycard?> GetLoyaltycardByUserIdAsync(int userId)
        {
            return await _context.Loyaltycards.FirstOrDefaultAsync(l => l.UserId == userId);
        }

        public async Task UpdatePointsAsync(int userId, int pointsChange)
        {
            var card = await _context.Loyaltycards.FirstOrDefaultAsync(l => l.UserId == userId);
            if (card != null)
            {
                card.PointsBalance = (card.PointsBalance ?? 0) + pointsChange;
                await _context.SaveChangesAsync();
            }
        }
    }
}
