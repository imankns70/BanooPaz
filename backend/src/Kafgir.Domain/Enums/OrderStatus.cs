namespace Kafgir.Domain.Enums;

public enum OrderStatus
{
    PendingConfirmation = 1,
    Confirmed = 2,
    Preparing = 3,
    Ready = 4,
    Delivered = 5,
    Cancelled = 6
}
