namespace Restaurant.Domain.Entities;

public class Chef
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsAvailable { get; set; } = true;
}