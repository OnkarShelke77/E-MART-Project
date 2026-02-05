using System.Security.Claims;
using EMart.Data;
using EMart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMart.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/address")]
    public class AddressController : ControllerBase
    {
        private readonly EMartDbContext _context;

        public AddressController(EMartDbContext context)
        {
            _context = context;
        }

        private string? UserEmail =>
            User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.Identity?.Name;

        // 🔹 SAVE ADDRESS DURING CHECKOUT
        [HttpPost("add")]
        public async Task<ActionResult<Address>> AddAddress([FromBody] Address address)
        {
            if (string.IsNullOrEmpty(UserEmail))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == UserEmail);
            if (user == null)
                return NotFound("User not found");

            address.UserId = user.Id;

            // Check for duplicate address
            var existingAddress = await _context.Addresses.FirstOrDefaultAsync(a =>
                a.UserId == user.Id
                && a.FullName == address.FullName
                && a.Mobile == address.Mobile
                && a.HouseNo == address.HouseNo
                && a.Street == address.Street
                && a.City == address.City
                && a.State == address.State
                && a.Pincode == address.Pincode
            );

            // If found, reuse it and update User's current address reference
            if (existingAddress != null)
            {
                user.Address =
                    $"{existingAddress.HouseNo}, {existingAddress.City}, {existingAddress.State} - {existingAddress.Pincode}";
                await _context.SaveChangesAsync();
                return Ok(existingAddress);
            }

            // Sync to User entity for Invoice service to pick up
            user.Address =
                $"{address.HouseNo}, {address.City}, {address.State} - {address.Pincode}";

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return Ok(address);
        }

        // 🔹 GET USER ADDRESSES
        [HttpGet("my")]
        public async Task<ActionResult<List<Address>>> GetMyAddresses()
        {
            if (string.IsNullOrEmpty(UserEmail))
                return Unauthorized();

            var addresses = await _context
                .Addresses.Where(a => a.User != null && a.User.Email == UserEmail)
                .ToListAsync();

            return Ok(addresses);
        }
    }
}
