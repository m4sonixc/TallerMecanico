using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TallerMecanico.Models;

[Table("turnos")]
public class Turno : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("usuario_id")]
    public long UsuarioId { get; set; }

    [Column("marca_vehiculo")]
    public string MarcaVehiculo { get; set; } = string.Empty;

    [Column("modelo_vehiculo")]
    public string ModeloVehiculo { get; set; } = string.Empty;

    [Column("tipo_servicio")]
    public string TipoServicio { get; set; } = "Reparación"; // "Service" o "Reparación"

    [Column("fecha_turno")]
    public DateTime FechaTurno { get; set; } = DateTime.Today;

    [Column("problema")]
    public string Problema { get; set; } = string.Empty;

    [Column("diagnostico")]
    public string Diagnostico { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = "Solicitado"; // Solicitado, Aceptado, Reprogramado, En Proceso, Finalizado, Cancelado

    [Column("fecha_estimada_entrega")]
    public DateTime? FechaEstimadaEntrega { get; set; }

    [Column("fecha_finalizacion")]
    public DateTime? FechaFinalizacion { get; set; }

    [Column("fecha_proximo_service")]
    public DateTime? FechaProximoService { get; set; }

    [Column("mano_obra")]
    public decimal ManoObra { get; set; }

    [Column("patente")]
    public string Patente { get; set; } = string.Empty;

    [Column("costo_total")]
    public decimal CostoTotal { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    // Propiedades de UI
    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string NombreCliente { get; set; } = string.Empty;

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string TelefonoCliente { get; set; } = string.Empty;

    [JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public string FechaFinalizacionFormateada =>
    (FechaFinalizacion ?? CreatedAt ?? DateTime.Now).ToString("dd/MM/yyyy HH:mm");
}