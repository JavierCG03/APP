using System.Globalization;

namespace CarslineApp.Converters;

/// <summary>
/// Convierte un valor booleano a texto descriptivo para expandir/contraer
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▲ Click para contraer" : "▼ Click para expandir";
        }
        return "▼ Click para expandir";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}