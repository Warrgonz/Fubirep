namespace fubi_api.Models
{
    public class Beneficiarios
    {
<<<<<<< Updated upstream
        

        public int id_beneficiario { get; set; }
        public int cedula { get; set; }
        public string? beneficiario {get; set; }
        public string? nombre { get; set; }
        public string? correo { get; set; }
        public int telefono { get; set; }
        public string? direccion { get; set; }
        public bool Activo { get; set; }
        public string? ruta_imagen { get; set; }
=======
        public int Id { get; set; }              // ID único del beneficiario
        public int Cedula { get; set; }          // Cédula del beneficiario
        public string? Beneficiario { get; set; } // Nombre completo del beneficiario
        public string? Correo { get; set; }       // Correo electrónico
        public int Telefono { get; set; }        // Número de teléfono
        public string? Direccion { get; set; }    // Dirección física
        public bool? Activo { get; set; }         // Si el beneficiario se encuentra activo o no
>>>>>>> Stashed changes
    }
}
