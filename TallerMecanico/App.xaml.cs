using Microsoft.Extensions.DependencyInjection;

namespace TallerMecanico
{
    // es la clase principal de la aplicación, que se encarga de inicializar la interfaz de usuario y establecer el tema de la aplicación.

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            UserAppTheme = AppTheme.Light;// establece el tema de la aplicación en modo claro
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell()); // crea una nueva ventana de la aplicación y establece la página principal como AppShell
        }
    }
}