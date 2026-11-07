using TallerMecanico.Models;

namespace TallerMecanico.Services;

public static class SessionService
{
    public static Usuario? CurrentUser { get; set; }

    public static void Logout()
    {
        CurrentUser = null;
    }
}