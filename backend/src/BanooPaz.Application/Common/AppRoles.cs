namespace BanooPaz.Application.Common;

public static class AppRoles
{
    public const string Customer = "Customer";
    public const string Owner = "Owner";
    public const string KitchenAdmin = "KitchenAdmin";
    public const string OrderManager = "OrderManager";

    public static readonly IReadOnlySet<string> AdminRoles = new HashSet<string>(StringComparer.Ordinal)
    {
        Owner,
        KitchenAdmin,
        OrderManager
    };

    public static readonly IReadOnlyList<string> All =
    [
        Customer,
        Owner,
        KitchenAdmin,
        OrderManager
    ];
}
