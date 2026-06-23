using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    public string Name
    {
        get => $"{FirstName} {LastName}".Trim();
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var parts = value.Split(' ', 2);
                FirstName = parts[0];
                LastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
        }
    }

    public string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Password { get; set; }

    public string? Address { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; }
    public DateTime? LastAssignedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
