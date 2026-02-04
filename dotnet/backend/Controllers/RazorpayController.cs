using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Text.Json;

namespace EMart.Controllers
{
    [ApiController]
    [Route("rzp")] // Matches Java @RequestMapping("/rzp")
    public class RazorpayController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RazorpayController(IConfiguration config)
        {
            _config = config;
        }

        private string KeyId => _config["Razorpay:KeyId"] ?? "";
        private string KeySecret => _config["Razorpay:KeySecret"] ?? "";

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok($"CheckoutController is active. KeyID: {KeyId}");
        }

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] JsonElement data)
        {
            try
            {
                double amount = data.GetProperty("amount").GetDouble();
                
                RazorpayClient client = new RazorpayClient(KeyId, KeySecret);

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (int)(amount * 100)); // Paise
                options.Add("currency", "INR");
                options.Add("receipt", "txn_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                Order order = client.Order.Create(options);

                var response = new
                {
                    id = order["id"].ToString(),
                    amount = order["amount"],
                    currency = order["currency"].ToString()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating Razorpay order: {ex.Message}");
            }
        }

        [HttpPost("verify-payment")]
        public IActionResult VerifyPayment([FromBody] Dictionary<string, string> data)
        {
            try
            {
                string orderId = data["razorpay_order_id"];
                string paymentId = data["razorpay_payment_id"];
                string signature = data["razorpay_signature"];

                RazorpayClient client = new RazorpayClient(KeyId, KeySecret);

                Dictionary<string, string> attributes = new Dictionary<string, string>();
                attributes.Add("razorpay_order_id", orderId);
                attributes.Add("razorpay_payment_id", paymentId);
                attributes.Add("razorpay_signature", signature);

                Utils.verifyPaymentSignature(attributes);

                return Ok(new { status = "success", message = "Payment verified" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = "error", message = $"Invalid signature: {ex.Message}" });
            }
        }
    }
}
