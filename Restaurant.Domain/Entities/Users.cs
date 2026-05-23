public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Password { get; set; }

    public string? Address { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }
    public DateTime? LastAssignedAt { get; set; }
}
