using System.Globalization;
using System.Windows.Data;
using Kafgir.Contracts.Orders;

namespace Kafgir.WPF.Converters;

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
