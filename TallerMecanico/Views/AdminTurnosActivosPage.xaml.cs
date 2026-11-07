using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class AdminTurnosActivosPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private List<Turno> _solicitadosOriginal = new();
    private List<Turno> _activosOriginal = new();

    public AdminTurnosActivosPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (SessionService.CurrentUser != null)
        {
            LblBienvenidaAdmin.Text = $"Bienvenido, {SessionService.CurrentUser.Nombre}";
        }

        await CargarTodosTurnos();
    }

    private async Task CargarTodosTurnos()
    {
        try
        {
            await _supabaseService.InitializeAsync();
            var turnosRes = await _supabaseService.Client.From<Turno>().Get();
            var usuariosRes = await _supabaseService.Client.From<Usuario>().Get();
            var dictUsuarios = usuariosRes.Models.ToDictionary(u => u.Id, u => u);

            var todos = turnosRes.Models.ToList();
            foreach (var t in todos)
            {
                if (dictUsuarios.TryGetValue(t.UsuarioId, out var user))
                {
                    t.NombreCliente = $"{user.Nombre} {user.Apellido}";
                    t.TelefonoCliente = user.Telefono;
                }
            }

            // 1. Turnos Solicitados (pendientes de aceptar por el mecánico)
            _solicitadosOriginal = todos
                .Where(t => t.Estado == "Solicitado")
                .OrderBy(t => t.FechaTurno)
                .ToList();

            // 2. Turnos Activos (ya aceptados, reprogramados o en proceso en taller)
            _activosOriginal = todos
                .Where(t => t.Estado == "Aceptado" || t.Estado == "Reprogramado" || t.Estado == "En Proceso")
                .OrderBy(t => t.FechaTurno)
                .ToList();

            // Contadores
            LblCountSolicitados.Text = _solicitadosOriginal.Count.ToString();
            LblCountActivos.Text = _activosOriginal.Count.ToString();

            FiltrarListas(SbTurnos.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private void OnBuscarTurnosChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarListas(e.NewTextValue);
    }

    private void FiltrarListas(string? filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            CvSolicitados.ItemsSource = _solicitadosOriginal;
            CvActivos.ItemsSource = _activosOriginal;
        }
        else
        {
            var f = filtro.Trim().ToLower();
            CvSolicitados.ItemsSource = _solicitadosOriginal.Where(t => CumpleFiltro(t, f)).ToList();
            CvActivos.ItemsSource = _activosOriginal.Where(t => CumpleFiltro(t, f)).ToList();
        }
    }

    private bool CumpleFiltro(Turno t, string f)
    {
        return (!string.IsNullOrEmpty(t.Patente) && t.Patente.ToLower().Contains(f)) ||
               (!string.IsNullOrEmpty(t.MarcaVehiculo) && t.MarcaVehiculo.ToLower().Contains(f)) ||
               (!string.IsNullOrEmpty(t.ModeloVehiculo) && t.ModeloVehiculo.ToLower().Contains(f)) ||
               (!string.IsNullOrEmpty(t.NombreCliente) && t.NombreCliente.ToLower().Contains(f));
    }

    // Toggle Desplegable: Turnos Solicitados
    private void OnToggleSolicitadosClicked(object? sender, EventArgs e)
    {
        PanelSolicitados.IsVisible = !PanelSolicitados.IsVisible;
        LblFlechaSolicitados.Text = PanelSolicitados.IsVisible ? "▼" : "▲";
    }

    // Toggle Desplegable: Turnos Activos
    private void OnToggleActivosClicked(object? sender, EventArgs e)
    {
        PanelActivos.IsVisible = !PanelActivos.IsVisible;
        LblFlechaActivos.Text = PanelActivos.IsVisible ? "▼" : "▲";
    }

    private async void OnHistorialTurnosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminHistorialPage));
    }

    private async void OnCatalogoRepuestosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminRepuestosPage));
    }

    private async void OnVentaRepuestosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminVentaRepuestosPage));
    }

    private async void OnBalanceClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AdminBalancePage));
    }

    private async void OnGestionarTurnoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Turno turnoSeleccionado)
        {
            var navParam = new Dictionary<string, object>
            {
                { "Turno", turnoSeleccionado }
            };
            await Shell.Current.GoToAsync(nameof(AdminDetalleTurnoPage), navParam);
        }
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        SessionService.Logout();
        Application.Current!.MainPage = new AppShell();
    }
}