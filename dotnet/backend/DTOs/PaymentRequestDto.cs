using System.ComponentModel.DataAnnotations;

namespace EMart.DTOs
{
    public class PaymentRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        [Required]
        public string PaymentMode { get; set; } = string.Empty;

        // ✅ ADD THESE (matching Java DTO)
        public string? PaymentStatus { get; set; }

        public string? TransactionId { get; set; }
    }
}
