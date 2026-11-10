using System.Globalization;

namespace TallerMecanico.Converters;

public class EstadoColorConverter : IValueConverter // en la base de datos, el estado del turno es un string, pero en la interfaz queremos mostrarlo con un color diferente segun el estado, por eso usamos este converter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value?.ToString();
        return estado switch// devuelve un color diferente segun el estado del turno
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
    // convertBack no se implementa porque no necesitamos convertir de color a estado
}