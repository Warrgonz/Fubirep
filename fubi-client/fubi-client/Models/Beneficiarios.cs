namespace fubi_client.Models
{
    public class Beneficiarios
    {

        public int Id { get; set; }              // ID único del beneficiario
        public int Cedula { get; set; }          // Cédula del beneficiario
        public string? Beneficiario { get; set; } // Nombre completo del beneficiario
        public string? Correo { get; set; }       // Correo electrónico
        public int Telefono { get; set; }        // Número de teléfono
        public string? Direccion { get; set; }    // Dirección física
        public bool? Activo { get; set; }         // Si el beneficiario se encuentra activo o no

        public string Nombre { get; set; }
    }
}
