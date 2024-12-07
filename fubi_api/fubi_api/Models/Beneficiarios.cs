namespace fubi_api.Models
{
    public class Beneficiarios
    {
        public int id_beneficiario { get; set; }
        public int cedula { get; set; }
        public string? beneficiario { get; set; }
        public string? Nombre { get; set; }
        public string? correo { get; set; }
        public int telefono { get; set; }
        public string? direccion { get; set; }
        public bool Activo { get; set; }

        public string?  ruta_imagen { get; set; }
    }
}
