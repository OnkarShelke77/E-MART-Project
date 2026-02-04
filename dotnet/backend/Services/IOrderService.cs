using EMart.Models;

namespace EMart.Services
{
    public interface IOrderService
    {
        Task<Ordermaster> PlaceOrderFromCartAsync(int userId, int? cartId, string paymentMode);
        Task<IEnumerable<Ordermaster>> GetAllOrdersAsync();
        Task<Ordermaster?> GetOrderByIdAsync(int id);
        Task<IEnumerable<Ordermaster>> GetOrdersByUserAsync(int userId);
    }
}
