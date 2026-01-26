using System.Globalization;

namespace CarslineApp.Converters
{
    // Convertidor para verificar si un objeto no es null
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertidor para verificar si un número es mayor que 1
    public class GreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int numero)
            {
                return numero > 1;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Convertidor de DateTime a TimeSpan para TimePicker
    public class DateTimeToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.TimeOfDay;
            }
            return TimeSpan.Zero;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return DateTime.Today.Add(timeSpan);
            }
            return DateTime.Now;
        }
    }
}