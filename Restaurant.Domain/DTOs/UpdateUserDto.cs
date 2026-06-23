namespace Restaurant.Application.DTOs;

public class UpdateUserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public int Role { get; set; }
    public string? Password { get; set; }
}
