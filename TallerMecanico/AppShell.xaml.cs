using TallerMecanico.Views;

namespace TallerMecanico;

public partial class AppShell : Shell
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
    }
}