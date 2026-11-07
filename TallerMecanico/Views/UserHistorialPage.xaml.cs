using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class UserHistorialPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public UserHistorialPage()
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
        if (SessionService.CurrentUser == null) return;

        try
        {
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client
                .From<Turno>()
                .Filter("usuario_id", Postgrest.Constants.Operator.Equals, SessionService.CurrentUser.Id.ToString())
                .Get();

            var historial = res.Models
                .Where(t => t.Estado == "Finalizado" || t.Estado == "Cancelado")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            CvHistorialUsuario.ItemsSource = historial;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
    private async void OnDescargarComprobanteClienteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Turno t)
        {
            try
            {
                // Obtener los repuestos si tuvo
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

                // Generar comprobante
                string rutaArchivo = await PdfService.GenerarComprobanteTurnoPdfAsync(t, repuestosTurno.Models);

                // Abrir selector nativo para guardar o compartir
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Comprobante - {t.Patente}",
                    File = new ShareFile(rutaArchivo)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Aceptar");
            }
        }
    }
}