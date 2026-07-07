using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BanooPaz.WPF.Converters;

public sealed class SelectedIndexToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int selectedIndex || parameter is null)
        {
            return Visibility.Collapsed;
        }

        return int.TryParse(parameter.ToString(), CultureInfo.InvariantCulture, out var expectedIndex)
            && selectedIndex == expectedIndex
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}
