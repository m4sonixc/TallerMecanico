using TallerMecanico.Views;

namespace TallerMecanico;

public partial class AppShell : Shell // contiene login, registro y olvido de contraseña
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(AdminDetalleTurnoPage), typeof(AdminDetalleTurnoPage));
        Routing.RegisterRoute(nameof(UserAgregarTurnoPage), typeof(UserAgregarTurnoPage));
    }// registra las rutas de navegación para las páginas de registro, olvido de contraseña, detalle de turno y agregar turno
}