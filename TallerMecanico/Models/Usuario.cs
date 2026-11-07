using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("usuarios")]
public class Usuario : BaseModel
{
    [PrimaryKey("id", false)] // <-- Solo aquí va el 'false' (shouldInsert)
    public long Id { get; set; }

    [Column("username")]
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