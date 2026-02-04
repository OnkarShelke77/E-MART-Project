using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EMart.DTOs;
using EMart.Services;
using System.Security.Claims;

namespace EMart.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/cartitem")] // Matches Java @RequestMapping("/api/cartitem")
    public class CartItemController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartItemController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string? UserEmail => User.FindFirst(ClaimTypes.Name)?.Value 
                                     ?? User.FindFirst("sub")?.Value 
                                     ?? User.Identity?.Name;

        [HttpPost("add")]
        public async Task<ActionResult<CartItemResponse>> AddCartItem([FromBody] CartItemRequest request)
        {
            if (UserEmail == null) return Unauthorized();
            try
            {
                var item = await _cartService.AddOrUpdateItemAsync(UserEmail, request);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<CartItemResponse>>> GetMyCartItems()
        {
            if (UserEmail == null) return Unauthorized();
            var cartResponse = await _cartService.GetUserCartAsync(UserEmail);
            return Ok(cartResponse.Items);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<CartItemResponse>> UpdateCartItem(int id, [FromBody] CartItemRequest request)
        {
            if (UserEmail == null) return Unauthorized();
            try
            {
                // In Java, it takes a DTO and uses quantity from it
                var item = await _cartService.UpdateQuantityAsync(UserEmail, id, request.Quantity);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            if (UserEmail == null) return Unauthorized();
            var success = await _cartService.RemoveItemAsync(UserEmail, id);
            if (!success) return NotFound(new { message = "Item not found or not authorized" });
            return Ok("CartItem deleted successfully"); // Matches Java return string
        }
    }
}
