using System.Text.Json.Serialization;

namespace fubi_client.Models
{
    public class TipoMovimiento
    {
        [JsonPropertyName("id_tipo_movimiento")]
        public int IdTipoMovimiento { get; set; }

        [JsonPropertyName("movimiento")]
        public string? Movimiento { get; set; }
    }
}