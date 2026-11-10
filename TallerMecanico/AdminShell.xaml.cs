using TallerMecanico.Views;

namespace TallerMecanico;

public partial class AdminShell : Shell //tiene como pagina raiz el panel admin , y contiene las paginas de detalle de turno, historial, repuestos, venta de repuestos y balance
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