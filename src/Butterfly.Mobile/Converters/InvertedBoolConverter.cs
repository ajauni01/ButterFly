using System.Globalization;

namespace Butterfly.Mobile.Converters;

/// <summary>Inverts a boolean for XAML bindings (e.g. show X only when a flag is false).</summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}
