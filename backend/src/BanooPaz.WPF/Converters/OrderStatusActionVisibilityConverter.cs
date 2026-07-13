using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BanooPaz.Contracts.Orders;

namespace BanooPaz.WPF.Converters;

public sealed class OrderStatusActionVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not OrderStatus status || parameter is not string action)
        {
            return Visibility.Collapsed;
        }

        var isVisible = action switch
        {
            "Confirm" => status == OrderStatus.PendingConfirmation,
            "Deliver" => status is OrderStatus.Confirmed or OrderStatus.Ready,
            "Cancel" => status is OrderStatus.PendingConfirmation or OrderStatus.Confirmed or OrderStatus.Preparing or OrderStatus.Ready,
            _ => false
        };

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
