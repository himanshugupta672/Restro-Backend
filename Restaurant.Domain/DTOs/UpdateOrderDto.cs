using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

public class UpdateOrderDto
{
    public OrderStatus Status { get; set; }

    public int? ChefId { get; set; }
}
