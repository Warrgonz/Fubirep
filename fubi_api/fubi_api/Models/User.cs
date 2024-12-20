
namespace fubi_api.Models
{
    public class User
    {
        public string? id_usuario { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? primer_apellido { get; set; }
        public string? segundo_apellido { get; set; }
        public string? correo { get; set; }
        public string? contrasena { get; set; }
        public string? contrasenaConfirmar { get; set; }
        public string? telefono { get; set; }
        public string? ruta_imagen { get; set; }
        public DateTime? fecha_nacimiento { get; set; }
        public string? rol { get; set; }
        public int activo { get; set; }

        // Utils
        public int id_rol { get; set; }
        public string Token { get; set; } = string.Empty;
        public string SecureToken { get; set; } = string.Empty;
        public DateTime? token_generado { get; set; }


    }

}
