using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class AdminRepuestosPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    private Repuesto? _repuestoSeleccionado;

    public AdminRepuestosPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarRepuestos();
    }

    private List<Repuesto> _todosRepuestos = new();
    private async Task CargarRepuestos()
    {
        try
        {
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client.From<Repuesto>().Get();
            _todosRepuestos = res.Models.OrderBy(r => r.Nombre).ToList();
            FiltrarRepuestos(SbRepuestos.Text);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }
    private void OnBuscarRepuestoChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarRepuestos(e.NewTextValue);
    }
    private void FiltrarRepuestos(string? filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
        {
            CvRepuestos.ItemsSource = _todosRepuestos;
        }
        else
        {
            var texto = filtro.Trim().ToLower();
            CvRepuestos.ItemsSource = _todosRepuestos
                .Where(r => r.Nombre.ToLower().Contains(texto))
                .ToList();
        }
    }

    private async void OnGuardarRepuestoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombreRepuesto.Text) ||
            !int.TryParse(TxtStockRepuesto.Text, out int stock) ||
            !decimal.TryParse(TxtPrecioRepuesto.Text, out decimal precio))
        {
            await DisplayAlert("Validación", "Completa todos los campos con valores correctos.", "Aceptar");
            return;
        }

        try
        {
            if (_repuestoSeleccionado == null)
            {
                var nuevo = new Repuesto
                {
                    Nombre = TxtNombreRepuesto.Text.Trim(),
                    Stock = stock,
                    PrecioUnitario = precio
                };
                await _supabaseService.Client.From<Repuesto>().Insert(nuevo);
                await DisplayAlert("Éxito", "Repuesto agregado al catálogo.", "Aceptar");
            }
            else
            {
                _repuestoSeleccionado.Nombre = TxtNombreRepuesto.Text.Trim();
                _repuestoSeleccionado.Stock = stock;
                _repuestoSeleccionado.PrecioUnitario = precio;
                await _supabaseService.Client.From<Repuesto>().Update(_repuestoSeleccionado);
                await DisplayAlert("Éxito", "Repuesto actualizado.", "Aceptar");
            }

            LimpiarFormulario();
            await CargarRepuestos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private void OnEditarRepuestoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Repuesto r)
        {
            _repuestoSeleccionado = r;
            TxtNombreRepuesto.Text = r.Nombre;
            TxtStockRepuesto.Text = r.Stock.ToString();
            TxtPrecioRepuesto.Text = r.PrecioUnitario.ToString("F2");
            BtnCancelarEdicion.IsVisible = true;
        }
    }

    private async void OnEliminarRepuestoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Repuesto r)
        {
            bool confirm = await DisplayAlert("Confirmar", $"¿Deseas eliminar '{r.Nombre}'?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    await _supabaseService.Client.From<Repuesto>().Delete(r);
                    await CargarRepuestos();
                }
                catch (Exception)
                {
                    await DisplayAlert("Error", "No se puede eliminar porque este repuesto ya fue utilizado en un turno.", "Aceptar");
                }
            }
        }
    }

    private void OnCancelarEdicionClicked(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        _repuestoSeleccionado = null;
        TxtNombreRepuesto.Text = string.Empty;
        TxtStockRepuesto.Text = string.Empty;
        TxtPrecioRepuesto.Text = string.Empty;
        BtnCancelarEdicion.IsVisible = false;
    }
}