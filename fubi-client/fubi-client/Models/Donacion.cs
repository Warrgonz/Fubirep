using System.Text.Json.Serialization;

namespace fubi_client.Models
{
    public class Donacion
    {
        [JsonPropertyName("Id_Donacion")]
        public int IdDonacion { get; set; }
        public int? IdTipoMovimiento { get; set; } //fk
        public int? IdTipoDonacion { get; set; } //fk
        public string? Donante { get; set; }
        public int? IdBeneficiario { get; set; } //fk
        public decimal? Monto { get; set; }
        public int? IdInventario { get; set; } //fk
        public int? Cantidad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;


    }
}
