using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

[QueryProperty(nameof(PatenteParam), "Patente")]
[QueryProperty(nameof(MarcaParam), "Marca")]
[QueryProperty(nameof(ModeloParam), "Modelo")]
[QueryProperty(nameof(TipoParam), "Tipo")]
public partial class UserAgregarTurnoPage : ContentPage
{
    public string PatenteParam { get; set; } = string.Empty;
    public string MarcaParam { get; set; } = string.Empty;
    public string ModeloParam { get; set; } = string.Empty;
    public string TipoParam { get; set; } = string.Empty;
    private readonly SupabaseService _supabaseService;

    public UserAgregarTurnoPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        DpFechaTurno.MinimumDate = DateTime.Today;
        DpFechaTurno.Date = DateTime.Today;
        if (SessionService.CurrentUser != null)
        {
            TxtNombreUser.Text = $"{SessionService.CurrentUser.Nombre} {SessionService.CurrentUser.Apellido}";
            TxtTelefonoUser.Text = SessionService.CurrentUser.Telefono;
        }
        // Si vienen datos precargados desde la alerta
        if (!string.IsNullOrEmpty(PatenteParam)) TxtPatente.Text = PatenteParam;
        if (!string.IsNullOrEmpty(MarcaParam)) TxtMarca.Text = MarcaParam;
        if (!string.IsNullOrEmpty(ModeloParam)) TxtModelo.Text = ModeloParam;
        if (!string.IsNullOrEmpty(TipoParam)) PickerTipoServicio.SelectedItem = TipoParam;
    }

    private async void OnConfirmarTurnoClicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(TxtPatente.Text) ||
            string.IsNullOrWhiteSpace(TxtMarca.Text) ||
            string.IsNullOrWhiteSpace(TxtModelo.Text) ||
            string.IsNullOrWhiteSpace(TxtProblema.Text))
        {
            await DisplayAlert("Validación", "Por favor completa la patente, marca, modelo y problema.", "Aceptar");
            return;
        }

        DateTime fechaElegida = DpFechaTurno.Date?.Date ?? DateTime.Today;

        try
        {
            await _supabaseService.InitializeAsync();

            // 1. Validar límite de 3 autos por fecha
            var fechaStr = fechaElegida.ToString("yyyy-MM-dd");
            var turnosDelDia = await _supabaseService.Client
                .From<Turno>()
                .Filter("fecha_turno", Postgrest.Constants.Operator.Equals, fechaStr)
                .Get();

            var turnosValidos = turnosDelDia.Models
                .Where(t => t.Estado != "Cancelado")
                .ToList();

            if (turnosValidos.Count >= 3)
            {
                await DisplayAlert("Cupo Completo",
                    $"Ya se alcanzó el límite máximo de 3 vehículos para el día {fechaElegida:dd/MM/yyyy}. Por favor selecciona otra fecha.",
                    "Aceptar");
                return;
            }

            // 2. Registrar el nuevo turno con estado "Solicitado"
            var nuevoTurno = new Turno
            {
                UsuarioId = SessionService.CurrentUser!.Id,
                MarcaVehiculo = TxtMarca.Text.Trim(),
                ModeloVehiculo = TxtModelo.Text.Trim(),
                Patente = TxtPatente.Text.Trim().ToUpper(),
                TipoServicio = PickerTipoServicio.SelectedItem?.ToString() ?? "Reparación",
                FechaTurno = fechaElegida,
                Problema = TxtProblema.Text.Trim(),
                Estado = "Solicitado",
                Diagnostico = string.Empty,
                ManoObra = 0,
                CostoTotal = 0
            };

            await _supabaseService.Client.From<Turno>().Insert(nuevoTurno);

            await DisplayAlert("Solicitud Enviada",
                "Tu turno fue enviado al taller con estado 'Solicitado'. El mecánico lo revisará y te confirmará o reprogramará la fecha.",
                "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}