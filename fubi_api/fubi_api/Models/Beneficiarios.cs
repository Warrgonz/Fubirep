namespace fubi_api.Models
{
    public class Beneficiarios
    {
        public int? Id { get; set; }              // ID único del beneficiario
        public int? Cedula { get; set; }          // Cédula del beneficiario
        public string? Beneficiario { get; set; } // Nombre completo del beneficiario
        public string? Correo { get; set; }       // Correo electrónico
        public int? Telefono { get; set; }        // Número de teléfono
        public string? Direccion { get; set; }    // Dirección física
        public string? RutaImagen { get; set; }   // Ruta de la imagen asociada
    }
}
