using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TallerMecanico.Models;

public class RepuestoVentaItem : INotifyPropertyChanged // avisa a la pantalla cuando cambia la cantidad de venta, para actualizar el subtotal sin tocar un boton
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int StockDisponible { get; set; }
    public decimal PrecioUnitario { get; set; }

    private int _cantidadVenta;
    public int CantidadVenta
    {
        get => _cantidadVenta;
        set
        {
            if (_cantidadVenta != value)
            {
                _cantidadVenta = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }
    }

    public decimal Subtotal => CantidadVenta * PrecioUnitario;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}