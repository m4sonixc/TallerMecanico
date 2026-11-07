using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class UserMisTurnosPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public UserMisTurnosPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (SessionService.CurrentUser != null)
        {
            LblBienvenida.Text = $"Bienvenido, {SessionService.CurrentUser.Nombre} {SessionService.CurrentUser.Apellido}";
        }

        await CargarMisTurnosActivos();
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await CargarMisTurnosActivos();
        MisTurnosRefresh.IsRefreshing = false;
    }

    private async Task CargarMisTurnosActivos()
    {
        if (SessionService.CurrentUser == null) return;
        try
        {
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client
                .From<Turno>()
                .Filter("usuario_id", Postgrest.Constants.Operator.Equals, SessionService.CurrentUser.Id.ToString())
                .Get();
            // AQUÍ VA: Carga todos los que NO estén Finalizados ni Cancelados (Solicitado, Aceptado, Reprogramado, En Proceso)
            var activos = res.Models
                .Where(t => t.Estado != "Finalizado" && t.Estado != "Cancelado")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            CvMisTurnos.ItemsSource = activos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
    private Turno? _turnoProximoService;

    private async Task VerificarAlertaProximoService()
    {
        if (SessionService.CurrentUser == null) return;

        try
        {
            var res = await _supabaseService.Client
                .From<Turno>()
                .Filter("usuario_id", Postgrest.Constants.Operator.Equals, SessionService.CurrentUser.Id.ToString())
                .Get();

            var hoy = DateTime.Today;
            var limiteAlerta = hoy.AddDays(15);

            // Buscamos algún vehículo cuyo próximo service esté a 15 días o ya vencido
            _turnoProximoService = res.Models
                .Where(t => t.FechaProximoService.HasValue && t.FechaProximoService.Value.Date <= limiteAlerta)
                .OrderBy(t => t.FechaProximoService)
                .FirstOrDefault();

            if (_turnoProximoService != null && _turnoProximoService.FechaProximoService.HasValue)
            {
                var fechaServ = _turnoProximoService.FechaProximoService.Value;
                string textoFecha = fechaServ < hoy ? "venció el " : "está programado para el ";

                LblMensajeAlertaService.Text = $"Tu [{_turnoProximoService.Patente}] {_turnoProximoService.MarcaVehiculo} {_turnoProximoService.ModeloVehiculo} tiene el service que {textoFecha} {fechaServ:dd/MM/yyyy}.";
                FrameAlertaService.IsVisible = true;
            }
            else
            {
                FrameAlertaService.IsVisible = false;
            }
        }
        catch { }
    }
    private async void OnAgendarServiceAlertaClicked(object? sender, EventArgs e)
    {
        if (_turnoProximoService != null)
        {
            var parametros = new Dictionary<string, object>
            {
                { "Patente", _turnoProximoService.Patente },
                { "Marca", _turnoProximoService.MarcaVehiculo },
                { "Modelo", _turnoProximoService.ModeloVehiculo },
                { "Tipo", "Service" }
            };
            await Shell.Current.GoToAsync(nameof(UserAgregarTurnoPage), parametros);
        }
    }
    private async void OnNuevoTurnoClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(UserAgregarTurnoPage));
    }

    private async void OnHistorialClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(UserHistorialPage));
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        SessionService.Logout();
        Application.Current!.MainPage = new AppShell();
    }
}