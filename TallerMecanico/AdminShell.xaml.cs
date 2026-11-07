using TallerMecanico.Views;

namespace TallerMecanico;

public partial class AdminShell : Shell
{
    public AdminShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AdminDetalleTurnoPage), typeof(AdminDetalleTurnoPage));
        Routing.RegisterRoute(nameof(AdminHistorialPage), typeof(AdminHistorialPage));
        Routing.RegisterRoute(nameof(AdminRepuestosPage), typeof(AdminRepuestosPage));
        Routing.RegisterRoute(nameof(AdminVentaRepuestosPage), typeof(AdminVentaRepuestosPage));
        Routing.RegisterRoute(nameof(AdminBalancePage), typeof(AdminBalancePage));
    }
}