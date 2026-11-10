using TallerMecanico.Models;
using TallerMecanico.Services;
using System.Collections.ObjectModel;

namespace TallerMecanico.Views;

[QueryProperty(nameof(TurnoActual), "Turno")]
public partial class AdminDetalleTurnoPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    public Turno TurnoActual { get; set; } = new();

    private List<Repuesto> _catalogoRepuestos = new();
    private ObservableCollection<TurnoRepuesto> _repuestosUsados = new();

    public AdminDetalleTurnoPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
        CvRepuestosAsignados.ItemsSource = _repuestosUsados;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InicializarVista();
    }

    private async Task InicializarVista()
    {
        LblVehiculo.Text = $"[{TurnoActual.Patente}] {TurnoActual.MarcaVehiculo} {TurnoActual.ModeloVehiculo}";
        LblCliente.Text = $"Cliente: {TurnoActual.NombreCliente} | Tel: {TurnoActual.TelefonoCliente}";
        LblTipoYFecha.Text = $"Tipo: {TurnoActual.TipoServicio} | Fecha del turno: {TurnoActual.FechaTurno:dd/MM/yyyy}";
        LblProblema.Text = $"Motivo reportado: {TurnoActual.Problema}";
        PanelAccionesTurno.IsVisible = TurnoActual.Estado == "Solicitado";
        PickerEstado.SelectedItem = TurnoActual.Estado;
        TxtDiagnostico.Text = TurnoActual.Diagnostico;
        TxtManoObra.Text = TurnoActual.ManoObra.ToString("F2");
        if (TurnoActual.Estado == "Finalizado" || TurnoActual.Estado == "Cancelado")
        {
            PanelAccionesTurno.IsVisible = false;
        }
        else
        {
            PanelAccionesTurno.IsVisible = true;
            // solo se ve si aún no fue aceptado 
            BtnAceptarTurno.IsVisible = TurnoActual.Estado == "Solicitado";
            // 'Reprogramar' y 'Cancelar' siempre visibles mientras el auto esté en el taller
            BtnReprogramarTurno.IsVisible = true;
            BtnCancelarTurno.IsVisible = true;
        }

        if (TurnoActual.FechaEstimadaEntrega.HasValue)
        {
            DpFechaEstimadaEntrega.Date = TurnoActual.FechaEstimadaEntrega.Value;
        }

        // si es service , muestra el panel de proximo service
        if (TurnoActual.TipoServicio.Equals("Service", StringComparison.OrdinalIgnoreCase))
        {
            PanelProximoService.IsVisible = true;
            DpProximoService.Date = TurnoActual.FechaProximoService ?? DateTime.Today.AddMonths(6);
        }

        await CargarCatalogoRepuestos();
        await CargarRepuestosTurno();
        ActualizarCostoTotal();
    }

    private void OnAceptarTurnoClicked(object sender, EventArgs e)
    {
        PickerEstado.SelectedItem = "Aceptado";
        TurnoActual.Estado = "Aceptado";

        // Ocultar botones de inmediato
        BtnAceptarTurno.IsVisible = false;
        FrameReprogramar.IsVisible = false;

        DisplayAlert("Turno Aceptado", "El turno fue marcado como 'Aceptado'. Recuerda presionar 'Guardar Cambios'.", "Aceptar");
    }

    private void OnReprogramarClicked(object sender, EventArgs e)
    {
        FrameReprogramar.IsVisible = !FrameReprogramar.IsVisible;
        DpNuevaFecha.Date = TurnoActual.FechaTurno.AddDays(1);
    }
    private async void OnCancelarTurnoClicked(object sender, EventArgs e)
    {
        // cuadro si y no
        bool seguro = await DisplayAlert("Confirmación", "¿Está seguro de cancelar el turno?", "Sí", "No");

        //si apreta no , vuelve 
        if (!seguro) return;

        try
        {
            await _supabaseService.InitializeAsync();

            // si es que habia respuestos asignados se devuelve, si no , no hace nada 
            if (_repuestosUsados.Any())
            {
                foreach (var repuestoAsignado in _repuestosUsados)
                {
                    var repuestoRes = await _supabaseService.Client
                        .From<Repuesto>()
                        .Filter("id", Postgrest.Constants.Operator.Equals, repuestoAsignado.RepuestoId.ToString())
                        .Get();

                    var repuestoReal = repuestoRes.Models.FirstOrDefault();
                    if (repuestoReal != null)
                    {
                        repuestoReal.Stock += repuestoAsignado.Cantidad;
                        await _supabaseService.Client.From<Repuesto>().Update(repuestoReal);
                    }
                }
            }

            // marca como cancelado y pone costo total y mano de obra en 0 y fecha de finalizacion en ahora
            TurnoActual.Estado = "Cancelado";
            TurnoActual.CostoTotal = 0;
            TurnoActual.ManoObra = 0;
            TurnoActual.FechaFinalizacion = DateTime.UtcNow;

            await _supabaseService.Client.From<Turno>().Update(TurnoActual);

            // cartel
            await DisplayAlert("Aviso", "Turno cancelado", "Aceptar");

            //inmediatamente vuelve a la pagina anterior
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
    private async void OnConfirmarReprogramacionClicked(object sender, EventArgs e)
    {
        DateTime nuevaFecha = DpNuevaFecha.Date ?? DateTime.Today;
        PanelAccionesTurno.IsVisible = false;
        try
        {
            await _supabaseService.InitializeAsync();
            var fechaStr = nuevaFecha.ToString("yyyy-MM-dd");
            var turnosDelDia = await _supabaseService.Client
                .From<Turno>()
                .Filter("fecha_turno", Postgrest.Constants.Operator.Equals, fechaStr)
                .Get();

            var turnosValidos = turnosDelDia.Models.Where(t => t.Estado != "Cancelado").ToList();
            if (turnosValidos.Count >= 3)
            {
                await DisplayAlert("Cupo Lleno", $"El día {nuevaFecha:dd/MM/yyyy} ya tiene el cupo completo de 3 autos.", "Aceptar");
                return;
            }

            TurnoActual.FechaTurno = nuevaFecha;
            TurnoActual.Estado = "Reprogramado";
            PickerEstado.SelectedItem = "Reprogramado";
            LblTipoYFecha.Text = $"Tipo: {TurnoActual.TipoServicio} | Fecha del turno: {TurnoActual.FechaTurno:dd/MM/yyyy}";
            FrameReprogramar.IsVisible = false;

            await DisplayAlert("Reprogramado", $"Fecha cambiada a {nuevaFecha:dd/MM/yyyy}. Presiona 'Guardar Cambios' para confirmar en la base de datos.", "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async Task CargarCatalogoRepuestos()
    {
        try
        {
            var res = await _supabaseService.Client.From<Repuesto>().Get();
            _catalogoRepuestos = res.Models.Where(r => r.Stock > 0).ToList();
            PickerRepuestos.ItemsSource = _catalogoRepuestos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error catálogo", ex.Message, "Aceptar");
        }
    }

    private async Task CargarRepuestosTurno()
    {
        try
        {
            var res = await _supabaseService.Client
                .From<TurnoRepuesto>()
                .Filter("turno_id", Postgrest.Constants.Operator.Equals, TurnoActual.Id.ToString())
                .Get();

            var repuestoNombres = (await _supabaseService.Client.From<Repuesto>().Get()).Models
                .ToDictionary(r => r.Id, r => r.Nombre);

            _repuestosUsados.Clear();
            foreach (var item in res.Models)
            {
                if (repuestoNombres.TryGetValue(item.RepuestoId, out var nom))
                {
                    item.NombreRepuesto = nom;
                }
                _repuestosUsados.Add(item);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error repuestos", ex.Message, "Aceptar");
        }
    }

    private void ActualizarCostoTotal()
    {
        decimal.TryParse(TxtManoObra.Text, out decimal manoObra);
        decimal sumaRepuestos = _repuestosUsados.Sum(r => r.Subtotal);
        decimal total = manoObra + sumaRepuestos;
        LblCostoTotal.Text = $"Costo Total: ${total:N2}";
    }// cada vez que se escribe un numero en mano de obra o se agrega un repuesto, se recalcula el costo total y se muestra en pantalla

    private void OnManoObraChanged(object sender, TextChangedEventArgs e)
    {
        ActualizarCostoTotal();
    }

    private async void OnAgregarRepuestoClicked(object sender, EventArgs e)
    {
        if (PickerRepuestos.SelectedItem is not Repuesto repuesto)
        {
            await DisplayAlert("Validación", "Selecciona un repuesto del catálogo.", "Aceptar");
            return;
        }

        if (!int.TryParse(TxtCantidadRepuesto.Text, out int cantidad) || cantidad <= 0)
        {
            await DisplayAlert("Validación", "Ingresa una cantidad válida mayor a cero.", "Aceptar");
            return;
        }

        if (cantidad > repuesto.Stock)
        {
            await DisplayAlert("Stock Insuficiente", $"Solo quedan {repuesto.Stock} unidades en stock.", "Aceptar");
            return;
        }

        try
        {
            decimal subtotal = cantidad * repuesto.PrecioUnitario;
            var nuevoTurnoRepuesto = new TurnoRepuesto
            {
                TurnoId = TurnoActual.Id,
                RepuestoId = repuesto.Id,
                Cantidad = cantidad,
                Subtotal = subtotal
            };

            var insertRes = await _supabaseService.Client.From<TurnoRepuesto>().Insert(nuevoTurnoRepuesto);
            var itemCreado = insertRes.Models.First();
            itemCreado.NombreRepuesto = repuesto.Nombre;

            repuesto.Stock -= cantidad;
            await _supabaseService.Client.From<Repuesto>().Update(repuesto);

            _repuestosUsados.Add(itemCreado);
            ActualizarCostoTotal();
            await CargarCatalogoRepuestos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al asignar repuesto", ex.Message, "Aceptar");
        }
    }

    private async void OnGuardarCambiosClicked(object sender, EventArgs e)
    {
        try
        {
            decimal.TryParse(TxtManoObra.Text, out decimal manoObra);
            decimal sumaRepuestos = _repuestosUsados.Sum(r => r.Subtotal);

            var nuevoEstado = PickerEstado.SelectedItem?.ToString() ?? "Solicitado";
            TurnoActual.Estado = nuevoEstado;
            TurnoActual.Diagnostico = TxtDiagnostico.Text ?? string.Empty;
            TurnoActual.ManoObra = manoObra;
            TurnoActual.CostoTotal = manoObra + sumaRepuestos;
            TurnoActual.FechaEstimadaEntrega = DpFechaEstimadaEntrega.Date;

            // si dice finalizado , guarda la fecha de finalizacion 
            if (nuevoEstado == "Finalizado")
            {
                TurnoActual.FechaFinalizacion = DateTime.UtcNow;
            }

            if (TurnoActual.TipoServicio.Equals("Service", StringComparison.OrdinalIgnoreCase))
            {
                TurnoActual.FechaProximoService = DpProximoService.Date;
            }

            await _supabaseService.Client.From<Turno>().Update(TurnoActual);

            await DisplayAlert("Guardado", "Turno actualizado correctamente.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al guardar", ex.Message, "Aceptar");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}