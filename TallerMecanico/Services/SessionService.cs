using TallerMecanico.Models;

namespace TallerMecanico.Services;

// guarda en la memoria de la aplicacion el usuario que esta logueado, para poder acceder a sus datos desde cualquier parte de la aplicacion
public static class SessionService
{
    public static Usuario? CurrentUser { get; set; }

    public static void Logout()
    {
        CurrentUser = null;
    }
}