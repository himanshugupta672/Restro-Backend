namespace Restaurant.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Assigned = 1,
    Accepted = 2,
    Preparing = 3,
    Ready = 4,
    Completed = 5,
    Cancelled = 6
}