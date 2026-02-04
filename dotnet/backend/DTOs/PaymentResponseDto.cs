namespace EMart.DTOs
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }

        public decimal AmountPaid { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; }

        // ✅ ADD THESE (used in service)
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserEmail { get; set; }
    }
}
