public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }
    public DateTime? LastAssignedAt { get; set; }
}