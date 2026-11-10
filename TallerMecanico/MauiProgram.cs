using TallerMecanico.Services;
using TallerMecanico.Views;

namespace TallerMecanico;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // es el primer archivo que se ejecuta al iniciar la aplicacion,, aqui se registran los servicios y las paginas de la aplicacion
        builder.Services.AddSingleton<SupabaseService>();

        //addtransient crea una nueva ventana cada vez que se llama a la pagina, esto es util para paginas que no necesitan mantener estado entre llamadas y liberar memoria cuando se cierran
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<AdminTurnosActivosPage>();
        builder.Services.AddTransient<AdminDetalleTurnoPage>();
        builder.Services.AddTransient<AdminHistorialPage>();
        builder.Services.AddTransient<AdminRepuestosPage>();
        builder.Services.AddTransient<AdminVentaRepuestosPage>();
        builder.Services.AddTransient<AdminBalancePage>();
        builder.Services.AddTransient<UserMisTurnosPage>();
        builder.Services.AddTransient<UserAgregarTurnoPage>();
        builder.Services.AddTransient<UserHistorialPage>();
        return builder.Build();
    }// crea y configura la aplicación MAUI, registrando servicios y páginas para la navegación
}