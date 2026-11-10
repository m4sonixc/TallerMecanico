using Supabase;
using TallerMecanico;
namespace TallerMecanico.Services;

public class SupabaseService
{
    // envuelve la funcionalidad de Supabase y proporciona una interfaz para interactuar con la base de datos y otros servicios de Supabase
    //client es el objeto principal que se utiliza para interactuar con la base de datos y otros servicios de Supabase
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
    // asyc task y await se utilizan para realizar operaciones asincronas, lo que permite que la aplicacion siga respondiendo mientras se realiza la operacion de inicializacion
    public async Task InitializeAsync()
    {
        await Client.InitializeAsync();
    }
}