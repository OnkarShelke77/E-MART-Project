namespace EMart.DTOs
{
    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(
        string FullName,
        string Email,
        string Password,
        string? Mobile,
        string? Address
    );

    public class LoginResponseDTO
    {
        public int? UserId { get; set; }
        public int? CartId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
    }

    public record GoogleLoginRequest(string Email, string FullName);
}
