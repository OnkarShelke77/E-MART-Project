using EMart.Models;

namespace EMart.Services
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId);
    }
}
