using System.Globalization;
using System.Windows.Data;
using BanooPaz.Domain.Enums;

namespace BanooPaz.Admin.Wpf.Converters;

public sealed class OrderStatusToPersianConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is OrderStatus status ? status switch
        {
            OrderStatus.PendingConfirmation => "در انتظار تایید",
            OrderStatus.Confirmed => "تایید شده",
            OrderStatus.Preparing => "در حال آماده‌سازی",
            OrderStatus.Ready => "آماده تحویل",
            OrderStatus.Delivered => "تحویل شده",
            OrderStatus.Cancelled => "لغو شده",
            _ => status.ToString()
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
