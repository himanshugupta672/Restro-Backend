public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}