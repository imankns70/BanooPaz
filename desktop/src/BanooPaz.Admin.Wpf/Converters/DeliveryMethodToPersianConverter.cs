using System.Globalization;
using System.Windows.Data;
using BanooPaz.Domain.Enums;

namespace BanooPaz.Admin.Wpf.Converters;

public sealed class DeliveryMethodToPersianConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is DeliveryMethod method ? method switch
        {
            DeliveryMethod.Pickup => "تحویل حضوری",
            DeliveryMethod.Delivery => "ارسال",
            _ => method.ToString()
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
