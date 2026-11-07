using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("turno_repuestos")]
public class TurnoRepuesto : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("turno_id")]
    public long TurnoId { get; set; }

    [Column("repuesto_id")]
    public long RepuestoId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Ignoramos esta propiedad para que NO se envíe en los INSERT/UPDATE de Supabase
    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string NombreRepuesto { get; set; } = string.Empty;
}