using TallerMecanico.Views;

namespace TallerMecanico;

public partial class UserShell : Shell // tiene como pagina raiz el panel de usuario , y contiene las paginas de agregar turno y historial
{
    public UserShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(UserAgregarTurnoPage), typeof(UserAgregarTurnoPage));
        Routing.RegisterRoute(nameof(UserHistorialPage), typeof(UserHistorialPage));
    }// registra las rutas de navegación para las páginas de agregar turno y historial
}