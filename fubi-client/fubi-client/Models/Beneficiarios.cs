namespace fubi_client.Models
{
    public class Beneficiarios
    {

        public int id_beneficiario { get; set; }              // ID único del beneficiario
        public int cedula { get; set; }          // Cédula del beneficiario
        public string? beneficiario { get; set; } // Nombre completo del beneficiario
        public string? correo { get; set; }       // Correo electrónico
        public int telefono { get; set; }        // Número de teléfono
        public string? direccion { get; set; }    // Dirección física
        public bool? activo { get; set; }         // Si el beneficiario se encuentra activo o no


    }
}
