using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("ventas_repuestos")]
public class VentaRepuesto : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("repuesto_id")]
    public long RepuestoId { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
}