using System.Globalization;

namespace TallerMecanico.Converters;

public class EstadoColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value?.ToString();
        return estado switch
        {
            "Solicitado" => Colors.DarkOrange,
            "Aceptado" => Colors.SeaGreen,
            "Reprogramado" => Colors.DarkMagenta,
            "En Proceso" => Colors.DodgerBlue,
            "Finalizado" => Colors.ForestGreen,
            "Cancelado" => Colors.Crimson,
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}