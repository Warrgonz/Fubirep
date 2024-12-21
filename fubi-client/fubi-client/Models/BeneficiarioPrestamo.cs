namespace fubi_client.Models
{
    public class BeneficiarioPrestamo
    {
        public int id_beneficiario { get; set; }             
        public string? cedula { get; set; }          
        public string? beneficiario { get; set; } 
        public string? correo { get; set; }       
        public string? telefono { get; set; }        
        public string? direccion { get; set; }    
        public int? activo { get; set; }         

    }
}
