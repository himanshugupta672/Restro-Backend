using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int TableId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public int? ChefId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public decimal TotalAmount { get; set; }

    public int? CustomerId { get; set; }
    public User? Customer { get; set; }

    public string? SpecialInstructions { get; set; }

    public int? EstimatedReadyMinutes { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}