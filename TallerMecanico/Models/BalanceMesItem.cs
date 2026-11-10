namespace TallerMecanico.Models;

public class BalanceMesItem 
{// representa un resumen de ingresos y estadísticas para un mes específico
    public string NombreMes { get; set; } = string.Empty; // Ej: "Noviembre 2026"
    public string RangoFechas { get; set; } = string.Empty; // Ej: "01/11/2026 al 30/11/2026"
    public decimal TotalMes { get; set; }
    public decimal IngresosVehiculos { get; set; }
    public decimal IngresosRepuestos { get; set; }
    public int AutosAtendidos { get; set; }
}