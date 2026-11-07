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

        // Registrar Servicio Supabase como Singleton
        builder.Services.AddSingleton<SupabaseService>();

        // Registrar Vistas
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
    }
}