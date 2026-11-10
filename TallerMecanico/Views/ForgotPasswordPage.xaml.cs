using TallerMecanico.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly SupabaseService _supabaseService;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _supabaseService = Handler?.MauiContext?.Services.GetService<SupabaseService>() ?? new SupabaseService();
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        var username = TxtUserRecup.Text?.Trim();
        var pass = TxtNuevaPass.Text?.Trim();
        // busca el usuario por username y si lo encuentra, actualiza la contraseña
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pass))
        {
            await DisplayAlert("Validación", "Por favor completa ambos campos.", "Aceptar");
            return;
        }

        if (pass.Length > 6)
        {
            await DisplayAlert("Validación", "La contraseña no puede superar los 6 caracteres.", "Aceptar");
            return;
        }

        try
        {
            await _supabaseService.InitializeAsync();
            var res = await _supabaseService.Client.From<Usuario>().Where(u => u.Username == username).Get();
            var user = res.Models.FirstOrDefault();

            if (user == null)
            {
                await DisplayAlert("Error", "No se encontró ningún usuario con ese nombre.", "Aceptar");
                return;
            }
            // actualiza la contraseña del usuario
            user.PasswordHash = pass;
            await _supabaseService.Client.From<Usuario>().Update(user);

            await DisplayAlert("Éxito", "Contraseña restablecida correctamente.", "Aceptar");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}