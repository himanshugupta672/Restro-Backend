using System.ComponentModel.DataAnnotations;

public class CreateOrderDto
{
    [Range(1, int.MaxValue)]
    public int TableNumber { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    [Range(1, int.MaxValue)]
    public int MenuItemId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}
