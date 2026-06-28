namespace Restaurant.Application.DTOs;

public class ForgotPasswordDto
{
    public string Email { get; set; }

    public string Otp { get; set; }

    public string Password { get; set; }

    public string ConfirmPassword { get; set; }
}
