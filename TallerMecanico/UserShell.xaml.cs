using TallerMecanico.Views;

namespace TallerMecanico;

public partial class UserShell : Shell
{
    public UserShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(UserAgregarTurnoPage), typeof(UserAgregarTurnoPage));
        Routing.RegisterRoute(nameof(UserHistorialPage), typeof(UserHistorialPage));
    }
}