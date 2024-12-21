namespace fubi_client.Models
{
    public class Beneficiarios
    {
        public int id_beneficiario { get; set; }
        public string? Cedula { get; set; }
        public string? Beneficiario { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int Activo { get; set; }
    }
}
