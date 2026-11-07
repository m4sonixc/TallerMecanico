using Supabase;
using TallerMecanico;
namespace TallerMecanico.Services;

public class SupabaseService
{
    public Client Client { get; private set; }

    public SupabaseService()
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        };
        Client = new Client(Constants.SupabaseUrl, Constants.SupabaseAnonKey, options);
    }

    public async Task InitializeAsync()
    {
        await Client.InitializeAsync();
    }
}