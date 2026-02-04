using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMart.DTOs;
using EMart.Services;

namespace EMart.Controllers
{
    [Authorize]
    [ApiController]
    [Route("payments")] // Matches Java @RequestMapping("/payments")
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDTO>> Create([FromBody] PaymentRequestDTO dto)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<PaymentResponseDTO>>> GetAll()
        {
            return await _paymentService.GetAllPaymentsAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponseDTO>> GetById(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<PaymentResponseDTO>>> GetByUser(int userId)
        {
            return await _paymentService.GetPaymentsByUserAsync(userId);
        }
    }
}
