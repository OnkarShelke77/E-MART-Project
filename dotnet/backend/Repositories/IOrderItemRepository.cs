using EMart.Models;

namespace EMart.Repositories
{
    public interface IOrderItemRepository
    {
        Task SaveAllAsync(IEnumerable<OrderItem> items);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
    }
}
