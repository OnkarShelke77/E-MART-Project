using EMart.Models;

namespace EMart.Repositories
{
    public interface IOrderRepository
    {
        Task<Ordermaster> SaveAsync(Ordermaster order);
        Task<IEnumerable<Ordermaster>> GetAllAsync();
        Task<Ordermaster?> GetByIdAsync(int id);
        Task<IEnumerable<Ordermaster>> GetByUserIdAsync(int userId);
    }
}
