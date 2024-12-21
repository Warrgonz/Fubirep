using System.Text.Json.Serialization;

namespace fubi_client.Models
{
    public class DonacionExtendida
    {
        [JsonPropertyName("Id_Donacion")]
        public int IdDonacion { get; set; }
        public string TipoMovimiento { get; set; } 
        [JsonPropertyName("Tipo_Donacion")]
        public string TipoDonacion { get; set; }  
        public string Donante { get; set; }
        public string Beneficiario { get; set; }  
        public decimal? Monto { get; set; }
        public string Inventario { get; set; }    
        public int? Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }
}
