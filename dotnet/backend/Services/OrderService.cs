using EMart.Data;
using EMart.Models;
using EMart.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EMart.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly ILoyaltycardService _loyaltycardService;
        private readonly EMartDbContext _context;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            ILoyaltycardService loyaltycardService,
            EMartDbContext context)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _loyaltycardService = loyaltycardService;
            _context = context;
        }

        public async Task<Ordermaster> PlaceOrderFromCartAsync(int userId, int? cartId, string paymentMode)
        {
            // Step 1: Check User exists
            var user = await _context.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
                throw new Exception($"User not found with id: {userId}");

            // Step 2: Fetch all cart items
            if (cartId == null && user.Cart != null)
            {
                cartId = user.Cart.Id;
            }

            if (cartId == null)
            {
                throw new Exception("Cart ID is missing and could not be resolved.");
            }

            var cartItems = await _context.Cartitems
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.Product)
                .ToListAsync();

            if (!cartItems.Any())
            {
                throw new Exception("Cart is empty. Cannot place order.");
            }

            // Step 3: Calculate Total Amount
            decimal totalAmount = 0;
            int totalPointsRequired = 0;

            foreach (var item in cartItems)
            {
                decimal price = item.PriceSnapshot;
                totalAmount += price * item.Quantity;

                int productPoints = item.Product.PointsToBeRedeem ?? 0;
                if (productPoints > 0)
                {
                    totalPointsRequired += productPoints * item.Quantity;
                }
            }

            // Step 4: Handle Points Redemption
            decimal amountPaidByPoints = 0;

            if ("LOYALTY".Equals(paymentMode, StringComparison.OrdinalIgnoreCase))
            {
                var card = await _loyaltycardService.GetLoyaltycardByUserIdAsync(userId);

                if (card == null)
                {
                    throw new Exception("Loyalty card not found");
                }

                if ((card.PointsBalance ?? 0) < totalPointsRequired)
                {
                    throw new Exception($"Insufficient loyalty points. Required: {totalPointsRequired}, Available: {card.PointsBalance}");
                }

                // 1 point = ₹1
                amountPaidByPoints = (decimal)totalPointsRequired;

                // Deduct points
                await _loyaltycardService.UpdatePointsAsync(userId, -totalPointsRequired);
            }

            decimal amountPaidByCash = totalAmount - amountPaidByPoints;

            // BLOCK POINTS-ONLY PURCHASE (MANDATORY RULE)
            if (amountPaidByCash <= 0)
            {
                throw new Exception("Points-only purchase is not allowed. Please pay some amount by cash.");
            }

            // Step 5: Create OrderMaster
            var ordermaster = new Ordermaster
            {
                UserId = userId,
                PaymentMode = paymentMode,
                OrderStatus = "Pending",
                TotalAmount = totalAmount,
                OrderDate = DateTime.UtcNow,
                AmountPaidByCash = amountPaidByCash,
                AmountPaidByPoints = amountPaidByPoints
            };

            // Step 6: Save OrderMaster
            var savedOrder = await _orderRepository.SaveAsync(ordermaster);

            // Step 7: Create OrderItems from cart items
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = savedOrder.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.PriceSnapshot
                };
                orderItems.Add(orderItem);
            }

            // Step 8: Save all OrderItems
            await _orderItemRepository.SaveAllAsync(orderItems);

            // Step 9: Clear cart after order is placed
            _context.Cartitems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Step 10: Add Loyalty Points (10% of total amount)
            try
            {
                int pointsEarned = (int)(totalAmount * 0.10m);
                if (pointsEarned > 0)
                {
                    await _loyaltycardService.UpdatePointsAsync(userId, pointsEarned);
                }
            }
            catch (Exception ex)
            {
                // Do NOT rollback order for reward failure
                Console.Error.WriteLine($"Loyalty points credit failed: {ex.Message}");
            }

            return savedOrder;
        }

        public async Task<IEnumerable<Ordermaster>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<Ordermaster?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Ordermaster>> GetOrdersByUserAsync(int userId)
        {
            return await _orderRepository.GetByUserIdAsync(userId);
        }
    }
}
