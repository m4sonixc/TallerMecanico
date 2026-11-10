using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("usuarios")] // con que tabla de supabase se conecta esta clase
public class Usuario : BaseModel // sin esto supabase no sabria leer ni guardar
{
    [PrimaryKey("id", false)] // lleva false porque supabase genera el id automaticamente
    public long Id { get; set; }

    [Column("username")]// conecta la propiedad con la columna de la tabla de supabase
    public string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [Column("rol")]
    public string Rol { get; set; } = "usuario";

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}