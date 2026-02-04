using Microsoft.AspNetCore.Mvc;
using EMart.Models;
using EMart.DTOs;
using EMart.Services;

namespace EMart.Controllers
{
    [ApiController]
    [Route("auth")] // Matches Java @RequestMapping("/auth")
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IJwtTokenService jwtService, IEmailService emailService)
        {
            _userService = userService;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // ---------------- NORMAL REGISTER ----------------
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDTO>> Register([FromBody] User user)
        {
            try 
            {
                var savedUser = await _userService.RegisterAsync(user);

                var response = new LoginResponseDTO
                {
                    UserId = savedUser.Id,
                    CartId = savedUser.Cart?.Id,
                    FullName = savedUser.FullName,
                    Email = savedUser.Email,
                    Message = "Registration successful"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ---------------- NORMAL LOGIN ----------------
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // ✅ Authenticate User
                var user = await _userService.LoginAsync(request.Email, request.Password);

                if (user == null) return Unauthorized(new { message = "Invalid credentials" });

                // ✅ Send Login Email
                await _emailService.SendLoginSuccessMailAsync(user);

                // ✅ Generate JWT Token
                var token = _jwtService.GenerateToken(user);

                // ✅ Prepare Response
                var response = new LoginResponseDTO
                {
                    UserId = user.Id,
                    CartId = user.Cart?.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Token = token,
                    Message = "Login successful + Email Sent!"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // ---------------- GOOGLE LOGIN (SSO) ----------------
        [HttpPost("google")]
        public async Task<ActionResult<LoginResponseDTO>> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                // Auto create user if first time
                var user = await _userService.LoginWithGoogleAsync(request.Email, request.FullName);

                if (user == null) return BadRequest(new { message = "Google login failed" });

                // ✅ Send Login Email
                await _emailService.SendLoginSuccessMailAsync(user);

                // Generate JWT Token
                var token = _jwtService.GenerateToken(user);

                // Response
                var response = new LoginResponseDTO
                {
                    UserId = user.Id,
                    CartId = user.Cart?.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Token = token,
                    Message = "Google login successful + Email Sent!"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
