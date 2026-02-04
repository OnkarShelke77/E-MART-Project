using System.Collections.Generic;
using System.Threading.Tasks;
using EMart.DTOs;
using EMart.Services;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        // Constructor Injection (equivalent to @Autowired)
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // ✅ CREATE payment
        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment(
            [FromBody] PaymentRequestDto dto
        )
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            return Ok(result);
        }

        // ✅ GET all payments
        [HttpGet]
        public async Task<ActionResult<List<PaymentResponseDto>>> GetAllPayments()
        {
            var result = await _paymentService.GetAllPaymentsAsync();
            return Ok(result);
        }

        // ✅ GET payment by id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(result);
        }

        // ✅ GET payments by user id
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<PaymentResponseDto>>> GetPaymentsByUser(int userId)
        {
            var result = await _paymentService.GetPaymentsByUserAsync(userId);
            return Ok(result);
        }
    }
}
