using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class LoginPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public LoginPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = TxtUsuario.Text?.Trim();
        var pass = TxtPassword.Text?.Trim();
        // toma lo que el usuario escribio
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pass))
        { // verifica que no esten vacios
            await DisplayAlert("Validación", "Por favor ingresa usuario y contraseña.", "Aceptar");
            return;
        }

        if (pass.Length > 6)
        {
            await DisplayAlert("Validación", "La contraseña no debe superar los 6 caracteres.", "Aceptar");
            return;
        }

        LoadingIndicator.IsVisible = true; // muestra el indicador de carga
        LoadingIndicator.IsRunning = true;

        try
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<Usuario>()
                .Filter("username", Postgrest.Constants.Operator.Equals, username)
                .Filter("password_hash", Postgrest.Constants.Operator.Equals, pass)
                .Get(); // consulta a la base de datos para verificar el usuario y la contraseña

            var user = response.Models.FirstOrDefault(); // toma el primer usuario que coincida 

            if (user != null)
            {
                SessionService.CurrentUser = user;// si existe el usuario, lo guarda en la sesión ,
                if (user.Rol.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    Application.Current!.MainPage = new AdminShell(); // si es admin, lo redirige al panel de admin
                }
                else
                {
                    Application.Current!.MainPage = new UserShell(); // si es usuario normal, lo redirige al panel de usuario
                }
            }
            else
            {
                await DisplayAlert("Acceso Denegado", "Usuario o contraseña incorrectos.", "Aceptar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error de conexión", ex.Message, "Aceptar");
        }
        finally // apaga la rueda de carga aunque haya habido un error
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
    }
}