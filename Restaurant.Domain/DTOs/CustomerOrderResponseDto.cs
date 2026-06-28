using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

public class CustomerOrderResponseDto
{
    public int OrderId { get; set; }

    public int TableId { get; set; }

    public int TableNumber { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? EstimatedTimeMinutes { get; set; }

    public string? EstimatedReadyAt { get; set; }

    public string? SpecialInstructions { get; set; }

    public List<CustomerOrderItemResponseDto> Items { get; set; } = [];
}

public class CustomerOrderItemResponseDto
{
    public int MenuItemId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal LineTotal { get; set; }
}
