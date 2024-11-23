namespace fubi_client.Models
{
    public class Beneficiarios
    {
        

        public int? Id { get; set; }
        public int? cedula { get; set; }
        public string? Beneficiario {get; set; }
        public string? Correo { get; set; }
        public int? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? ruta_imagen { get; set; }
    }
}