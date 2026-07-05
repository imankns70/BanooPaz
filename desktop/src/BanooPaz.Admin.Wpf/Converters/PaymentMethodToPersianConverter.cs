using System.Globalization;
using System.Windows.Data;
using BanooPaz.Contracts.Orders;

namespace BanooPaz.Admin.Wpf.Converters;

public sealed class PaymentMethodToPersianConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is PaymentMethod method ? method switch
        {
            PaymentMethod.Cash => "نقدی",
            PaymentMethod.CardToCard => "کارت‌به‌کارت",
            PaymentMethod.Online => "آنلاین",
            _ => method.ToString()
        } : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
