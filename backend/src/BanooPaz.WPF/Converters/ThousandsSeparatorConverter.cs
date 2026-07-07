using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BanooPaz.WPF.Converters;

public sealed class ThousandsSeparatorConverter : IValueConverter
{
    private static readonly CultureInfo SeparatorCulture = CultureInfo.InvariantCulture;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            decimal decimalValue => decimalValue.ToString("N0", SeparatorCulture),
            int intValue => intValue.ToString("N0", SeparatorCulture),
            long longValue => longValue.ToString("N0", SeparatorCulture),
            null => string.Empty,
            _ => value
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return targetType == typeof(decimal) ? 0m : 0;
        }

        var normalizedValue = NormalizeDigits(value.ToString() ?? string.Empty)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("٬", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return targetType == typeof(decimal) ? 0m : 0;
        }

        if (targetType == typeof(decimal) && decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return decimalValue;
        }

        if (targetType == typeof(int) && int.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
        {
            return intValue;
        }

        if (targetType == typeof(long) && long.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            return longValue;
        }

        return Binding.DoNothing;
    }

    private static string NormalizeDigits(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character switch
            {
                >= '۰' and <= '۹' => (char)('0' + character - '۰'),
                >= '٠' and <= '٩' => (char)('0' + character - '٠'),
                _ => character
            });
        }

        return builder.ToString();
    }
}
