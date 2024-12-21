using System.Text.Json.Serialization;

namespace fubi_client.Models
{
    public class Beneficiarios
    {
        public int id_beneficiario { get; set; }
        [JsonPropertyName("cedula")]
        public string? Cedula { get; set; }
        [JsonPropertyName("beneficiario")]
        public string? Beneficiario { get; set; }
        [JsonPropertyName("correo")]
        public string? Correo { get; set; }
        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }
        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }
        [JsonPropertyName("activo")]
        public int Activo { get; set; }
    }
}
