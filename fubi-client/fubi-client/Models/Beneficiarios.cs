namespace fubi_client.Models
{
    public class Beneficiarios
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Beneficiario {get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string RutaImagen { get; set; }
    }
}