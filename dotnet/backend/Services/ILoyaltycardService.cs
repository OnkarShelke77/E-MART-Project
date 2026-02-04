using EMart.Models;

namespace EMart.Services
{
    public interface ILoyaltycardService
    {
        Task<Loyaltycard?> GetLoyaltycardByUserIdAsync(int userId);
        Task UpdatePointsAsync(int userId, int pointsChange);
    }
}
