using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("repuestos")]
public class Repuesto : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("stock")]
    public int Stock { get; set; }

    [Column("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool EsStockBajo => Stock <= 3;

}