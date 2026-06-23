namespace Restaurant.Application.DTOs;

public class SendOtpRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class VerifyOtpRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Otp { get; set; }
}
