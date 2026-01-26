using System.Globalization;

namespace CarslineApp.Converters;

/// <summary>
/// Convierte el estado de un trabajo a un color de fondo apropiado
/// </summary>
public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string estado)
            return Colors.Gray;

        return estado switch
        {
            "Asignado" => Color.FromArgb("#FFEBEE"),      // Rojo claro
            "En Proceso" => Color.FromArgb("#FFF3E0"),     // Naranja claro
            "Pausado" => Color.FromArgb("#FFFDE7"),        // Amarillo claro
            "Completado" => Color.FromArgb("#E8F5E9"),     // Verde claro
            _ => Color.FromArgb("#F5F5F5")                 // Gris claro
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}