using System.Globalization;

namespace CarslineApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return Color.FromArgb("#D60000"); // Color activo
            }
            return Color.FromArgb("#757575"); // Color inactivo
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}