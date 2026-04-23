namespace Restaurant.Domain.Entities;

public class Table
{
    public int Id { get; set; }

    public int TableNumber { get; set; }

    public bool IsActive { get; set; }
}