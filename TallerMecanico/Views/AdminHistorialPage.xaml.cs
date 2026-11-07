using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class AdminHistorialPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private List<Turno> _todosHistorial = new();

    public AdminHistorialPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarHistorial();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await CargarHistorial();
        HistorialRefresh.IsRefreshing = false;
    }

    private async Task CargarHistorial()
    {
        try
        {
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client
                .From<Turno>()
                .Where(t => t.Estado == "Finalizado" || t.Estado == "Cancelado")
                .Get();

            var usuariosRes = await _supabaseService.Client.From<Usuario>().Get();
            var dictUsuarios = usuariosRes.Models.ToDictionary(u => u.Id, u => u);

            _todosHistorial = res.Models.OrderByDescending(t => t.CreatedAt).ToList();
            foreach (var t in _todosHistorial)
            {
                if (dictUsuarios.TryGetValue(t.UsuarioId, out var user))
                {
                    t.NombreCliente = $"{user.Nombre} {user.Apellido}";
                }
            }

            FiltrarHistorial(SbHistorial.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private void OnBuscarHistorialChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarHistorial(e.NewTextValue);
    }

    private void FiltrarHistorial(string? filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            CvHistorial.ItemsSource = _todosHistorial;
        }
        else
        {
            var f = filtro.Trim().ToLower();
            CvHistorial.ItemsSource = _todosHistorial.Where(t =>
                (!string.IsNullOrEmpty(t.Patente) && t.Patente.ToLower().Contains(f)) ||
                (!string.IsNullOrEmpty(t.MarcaVehiculo) && t.MarcaVehiculo.ToLower().Contains(f)) ||
                (!string.IsNullOrEmpty(t.ModeloVehiculo) && t.ModeloVehiculo.ToLower().Contains(f)) ||
                (!string.IsNullOrEmpty(t.NombreCliente) && t.NombreCliente.ToLower().Contains(f))
            ).ToList();
        }
    }
    private async void OnCompartirComprobanteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Turno t)
        {
            try
            {
                // Obtenemos los repuestos usados en este turno para el desglose del PDF
                var repuestosTurno = await _supabaseService.Client
                    .From<TurnoRepuesto>()
                    .Filter("turno_id", Postgrest.Constants.Operator.Equals, t.Id.ToString())
                    .Get();

                var todosRepuestos = (await _supabaseService.Client.From<Repuesto>().Get()).Models
                    .ToDictionary(r => r.Id, r => r.Nombre);

                foreach (var r in repuestosTurno.Models)
                {
                    if (todosRepuestos.TryGetValue(r.RepuestoId, out var nom))
                        r.NombreRepuesto = nom;
                }

                // Generar el archivo PDF
                string rutaPdf = await PdfService.GenerarComprobanteTurnoPdfAsync(t, repuestosTurno.Models);

                // Compartir el archivo PDF generado
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Comprobante de Servicio - {t.Patente}",
                    File = new ShareFile(rutaPdf)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error al generar PDF", ex.Message, "Aceptar");
            }
        }
    }
}