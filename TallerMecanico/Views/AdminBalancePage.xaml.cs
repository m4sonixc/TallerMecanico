using System.Globalization;
using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class AdminBalancePage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public AdminBalancePage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
        Loaded += async (s, e) => await CargarMetricasBalance();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarMetricasBalance();
    }

    private async void OnActualizarClicked(object sender, EventArgs e)
    {
        await CargarMetricasBalance();
    }

    private async Task CargarMetricasBalance()
    {
        try
        {
            await _supabaseService.InitializeAsync();

            // 1. Obtener Turnos y Ventas
            var turnosRes = await _supabaseService.Client.From<Turno>().Get();
            var todosTurnos = turnosRes.Models;

            var ventasRes = await _supabaseService.Client.From<VentaRepuesto>().Get();
            var todasVentas = ventasRes.Models;

            var cultureEs = new CultureInfo("es-ES");
            var hoy = DateTime.Today;

            // Fechas del mes actual (1ro al último día del mes)
            var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var finMesActual = inicioMesActual.AddMonths(1).AddDays(-1);

            string textoPeriodo = $"📅 {inicioMesActual:dd 'de' MMMM} al {finMesActual:dd 'de' MMMM 'de' yyyy}";
            // Capitalizar mes
            textoPeriodo = cultureEs.TextInfo.ToTitleCase(textoPeriodo);

            // Filtro para el mes actual
            var turnosMesActual = todosTurnos.Where(t =>
            {
                var f = t.FechaFinalizacion ?? t.CreatedAt ?? DateTime.MinValue;
                return f >= inicioMesActual && f <= finMesActual.AddDays(1);
            }).ToList();

            var finalizadosMes = turnosMesActual.Where(t => t.Estado == "Finalizado").ToList();
            var canceladosMes = turnosMesActual.Where(t => t.Estado == "Cancelado").ToList();

            decimal ingresosVehiculosMes = finalizadosMes.Sum(t => t.CostoTotal);
            int cantAtendidosMes = finalizadosMes.Count;
            int cantCanceladosMes = canceladosMes.Count;

            var ventasMesActual = todasVentas.Where(v =>
            {
                var f = (v.CreatedAt ?? DateTime.UtcNow).ToLocalTime();
                return f.Year == hoy.Year && f.Month == hoy.Month;
            }).ToList();

            decimal ingresosRepuestosMes = ventasMesActual.Sum(v => v.Subtotal);
            decimal totalMesActual = ingresosVehiculosMes + ingresosRepuestosMes;

            // 2. AGRUPAR TODOS LOS MESES PARA EL HISTORIAL INFERIOR
            var listaMeses = new List<BalanceMesItem>();

            
            var fechasTurnos = todosTurnos.Select(t => (t.FechaFinalizacion ?? t.CreatedAt ?? DateTime.UtcNow).ToLocalTime());
            var fechasVentas = todasVentas.Select(v => (v.CreatedAt ?? DateTime.UtcNow).ToLocalTime());
            var periodos = fechasTurnos.Concat(fechasVentas)
                .Select(f => new { f.Year, f.Month })
                .Distinct()
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToList();

            foreach (var p in periodos)
            {
                var inicioPeriodo = new DateTime(p.Year, p.Month, 1);
                var finPeriodo = inicioPeriodo.AddMonths(1).AddDays(-1);

                // Turnos del mes p
                var turnosDelMes = todosTurnos.Where(t =>
                {
                    var f = t.FechaFinalizacion ?? t.CreatedAt ?? DateTime.MinValue;
                    return f >= inicioPeriodo && f <= finPeriodo.AddDays(1);
                }).ToList();

                var finalizadosP = turnosDelMes.Where(t => t.Estado == "Finalizado").ToList();
                decimal ingVehiculosP = finalizadosP.Sum(t => t.CostoTotal);
                int autosP = finalizadosP.Count;

                // Ventas del mes p
                var ventasDelMes = todasVentas.Where(v =>
                {
                    var f = (v.CreatedAt ?? DateTime.UtcNow).ToLocalTime();
                    return f.Year == p.Year && f.Month == p.Month;
                }).ToList();

                decimal ingRepuestosP = ventasDelMes.Sum(v => v.Subtotal);
                decimal totalP = ingVehiculosP + ingRepuestosP;

                string nombreMesFormateado = cultureEs.TextInfo.ToTitleCase(inicioPeriodo.ToString("MMMM yyyy", cultureEs));
                string rangoStr = $"{inicioPeriodo:dd/MM/yyyy} al {finPeriodo:dd/MM/yyyy}";

                listaMeses.Add(new BalanceMesItem
                {
                    NombreMes = nombreMesFormateado,
                    RangoFechas = rangoStr,
                    TotalMes = totalP,
                    IngresosVehiculos = ingVehiculosP,
                    IngresosRepuestos = ingRepuestosP,
                    AutosAtendidos = autosP
                });
            }

            // 3. Alerta de Repuestos Críticos
            var repuestosRes = await _supabaseService.Client.From<Repuesto>().Get();
            int repuestosBajos = repuestosRes.Models.Count(r => r.Stock <= 3);

            // 4. Actualizar Interfaz en MainThread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LblPeriodoActual.Text = textoPeriodo;
                LblTotalDinero.Text = $"${totalMesActual:N2}";
                LblIngresosVehiculos.Text = $"${ingresosVehiculosMes:N2}";
                LblIngresosRepuestos.Text = $"${ingresosRepuestosMes:N2}";
                LblVehiculosAtendidos.Text = cantAtendidosMes.ToString();
                LblVehiculosCancelados.Text = cantCanceladosMes.ToString();

                if (repuestosBajos > 0)
                {
                    LblStockCriticoCount.Text = $"{repuestosBajos} repuestos con stock crítico (≤ 3 unidades).";
                    FrameStockAlerta.IsVisible = true;
                }
                else
                {
                    FrameStockAlerta.IsVisible = false;
                }

                CvHistorialMeses.ItemsSource = listaMeses;
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al calcular balance", ex.Message, "Aceptar");
        }
    }
}