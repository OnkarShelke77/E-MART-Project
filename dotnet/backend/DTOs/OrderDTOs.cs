namespace EMart.DTOs
{
    public class PlaceOrderRequest
    {
        public int UserId { get; set; }
        public int? CartId { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
    }
}
