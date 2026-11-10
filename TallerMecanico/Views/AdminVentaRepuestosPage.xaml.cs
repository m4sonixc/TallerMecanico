using System.Collections.ObjectModel;
using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class AdminVentaRepuestosPage : ContentPage
{// clase para la venta de repuestos, permite buscar repuestos, ingresar cantidades y realizar la venta, generando un ticket en PDF
    private readonly SupabaseService _supabaseService;
    private List<RepuestoVentaItem> _todosItemsVenta = new();// guarda en memoria todos los repuestos disponibles para poder filtrarlos sin tener que consultar la base de datos cada vez
    public ObservableCollection<RepuestoVentaItem> ItemsMostrados { get; set; } = new();// muestra los repuestos filtrados en la interfaz de usuario

    public AdminVentaRepuestosPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
        CvVentaRepuestos.ItemsSource = ItemsMostrados;
        //cargar los repuestos disponibles al cargar la página
        Loaded += async (s, e) => await CargarRepuestosDisponibles();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarRepuestosDisponibles();
    }

    private async Task CargarRepuestosDisponibles()
    {
        try
        {// carga todos los repuestos con stock mayor a 0 desde la base de datos y los ordena por nombre
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client.From<Repuesto>().Get();

            _todosItemsVenta = res.Models
                .Where(r => r.Stock > 0)
                .OrderBy(r => r.Nombre)
                .Select(r => new RepuestoVentaItem
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    StockDisponible = r.Stock,
                    PrecioUnitario = r.PrecioUnitario,
                    CantidadVenta = 0
                })
                .ToList();

            FiltrarRepuestos(SbVentaRepuestos.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private void OnBuscarVentaChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarRepuestos(e.NewTextValue);
    }

    private void FiltrarRepuestos(string? filtro)
    {// filtra los repuestos según el texto ingresado en el buscador y actualiza la lista mostrada en la interfaz de usuario
        List<RepuestoVentaItem> resultado;

        if (string.IsNullOrWhiteSpace(filtro))
        {
            resultado = _todosItemsVenta;
        }
        else
        {
            var texto = filtro.Trim().ToLower();
            resultado = _todosItemsVenta.Where(r => r.Nombre.ToLower().Contains(texto)).ToList();
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {//tiene permiso de actualizar la interfaz de usuario desde un hilo diferente al principal, evitando errores de acceso a la interfaz de usuario
            ItemsMostrados.Clear();
            foreach (var item in resultado)
            {
                ItemsMostrados.Add(item);
            }
            CalcularTotal();
        });
    }

    private void OnCantidadChanged(object sender, TextChangedEventArgs e)
    {
        CalcularTotal();
    }

    private void CalcularTotal()
    {
        decimal total = _todosItemsVenta.Sum(i => i.Subtotal);
        LblTotalVenta.Text = $"${total:N2}";
    }

    private async void OnRealizarVentaClicked(object sender, EventArgs e)
    {// obtiene los repuestos seleccionados para la venta y valida que las cantidades ingresadas sean válidas antes de proceder con la venta
        var seleccionados = _todosItemsVenta.Where(i => i.CantidadVenta > 0).ToList();

        if (!seleccionados.Any())
        {
            await DisplayAlert("Validación", "Debes ingresar al menos una cantidad mayor a 0 para vender.", "Aceptar");
            return;
        }

        foreach (var item in seleccionados)
        {
            if (item.CantidadVenta > item.StockDisponible)
            {
                await DisplayAlert("Stock Insuficiente", $"No puedes vender {item.CantidadVenta} unidades de '{item.Nombre}'. Stock disponible: {item.StockDisponible}.", "Aceptar");
                return;
            }
        }
        // calcula el total de la venta y solicita confirmación al usuario antes de proceder con la venta
        decimal totalVenta = seleccionados.Sum(i => i.Subtotal);
        bool confirmar = await DisplayAlert("Confirmar Venta", $"¿Confirmas la venta por un total de ${totalVenta:N2}?", "Sí, Vender", "Cancelar");
        if (!confirmar) return;

        try
        {
            await _supabaseService.InitializeAsync();

            foreach (var item in seleccionados)
            {
                var nuevaVenta = new VentaRepuesto
                {
                    RepuestoId = item.Id,
                    Cantidad = item.CantidadVenta,
                    PrecioUnitario = item.PrecioUnitario,
                    Subtotal = item.Subtotal,
                    CreatedAt = DateTime.UtcNow
                };
                await _supabaseService.Client.From<VentaRepuesto>().Insert(nuevaVenta);

                var repuestoRes = await _supabaseService.Client
                    .From<Repuesto>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, item.Id.ToString())
                    .Get();

                var repuesto = repuestoRes.Models.FirstOrDefault();
                if (repuesto != null)
                {
                    repuesto.Stock -= item.CantidadVenta;
                    await _supabaseService.Client.From<Repuesto>().Update(repuesto);
                }
            }

            // genera el ticket de venta en PDF y ofrece la opción de compartirlo
            string rutaTicket = await PdfService.GenerarTicketVentaRepuestosAsync(seleccionados, totalVenta);

            bool compartir = await DisplayAlert("Venta Exitosa", "¿Deseas descargar o compartir el comprobante / ticket?", "Sí, Compartir", "No");
            if (compartir)
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Ticket de Venta - PachiTaller",
                    File = new ShareFile(rutaTicket)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al procesar", ex.Message, "Aceptar");
        }
    }
}