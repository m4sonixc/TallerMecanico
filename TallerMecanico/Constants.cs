using System;
using System.Collections.Generic;
using System.Text;

namespace  TallerMecanico;
//static significa que se puede acceder a la clase sin necesidad de crear una instancia de la misma
public static class Constants
{
    // se guarda la direccion y la api key de supabase para poder acceder a la base de datos
    // const significa que el valor de la variable no puede cambiar en tiempo de ejecucion
    public const string SupabaseUrl = "https://urgqjwqjlcyhlszlxzap.supabase.co";
    //la url no debe terminar con / ya que supabase no lo permite y dara error al momento de hacer la conexion
    public const string SupabaseAnonKey = "sb_publishable_3TA8xQiiTk1Rxqyd-pgMMg_Ts_FMgL6";
}