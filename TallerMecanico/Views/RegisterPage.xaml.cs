using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class RegisterPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public RegisterPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    private async void OnRegisterSubmitClicked(object sender, EventArgs e)
    {
        var nombre = TxtNombre.Text?.Trim();
        var apellido = TxtApellido.Text?.Trim();
        var telefono = TxtTelefono.Text?.Trim();
        var username = TxtUsername.Text?.Trim();
        var password = TxtPassword.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nombre) ||
            string.IsNullOrWhiteSpace(apellido) ||
            string.IsNullOrWhiteSpace(telefono) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validación", "Por favor completa todos los campos.", "Aceptar");
            return;
        }

        if (password.Length > 6)
        {
            await DisplayAlert("Validación", "La contraseña debe tener máximo 6 caracteres.", "Aceptar");
            return;
        }

        try
        {
            await _supabaseService.InitializeAsync();

            // Usamos Filter nativo de Postgrest en lugar de lambda para evitar problemas de compatibilidad
            var checkRes = await _supabaseService.Client
                .From<Usuario>()
                .Filter("username", Postgrest.Constants.Operator.Equals, username)
                .Get();

            if (checkRes.Models.Count > 0)
            {
                await DisplayAlert("Aviso", "El nombre de usuario ya está registrado.", "Aceptar");
                return;
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Telefono = telefono,
                Username = username,
                PasswordHash = password,
                Rol = "usuario"
            };

            await _supabaseService.Client.From<Usuario>().Insert(nuevoUsuario);

            await DisplayAlert("Éxito", "Usuario registrado correctamente.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            // Mostramos el mensaje detallado o la excepción interna si existe
            var detalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            await DisplayAlert("Error", detalle, "Aceptar");
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}