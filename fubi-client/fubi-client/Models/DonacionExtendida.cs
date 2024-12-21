using System.Text.Json.Serialization;

namespace fubi_client.Models
{
    public class DonacionExtendida
    {
        [JsonPropertyName("Id_Donacion")]
        public int IdDonacion { get; set; }
        public string TipoMovimiento { get; set; } // Nombre del tipo de movimiento
        [JsonPropertyName("Tipo_Donacion")]
        public string TipoDonacion { get; set; }  // Nombre del tipo de donación
        public string Donante { get; set; }
        public string Beneficiario { get; set; }  // Nombre del beneficiario
        public decimal? Monto { get; set; }
        public string Inventario { get; set; }    // Nombre del inventario
        public int? Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }
}
