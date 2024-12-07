namespace fubi_api.Models
{
    public class PrestamoRequest
    {
        public int id_prestamo { get; set; }
        public int id_beneficiario { get; set; }
        public int id_encargado { get; set; }
        public int id_inventario { get; set; }
        public int cantidad { get; set; }
        public DateTime fecha_prestamo { get; set; }
        public DateTime fecha_limite_devolución { get; set; }
        public int id_estado { get; set; }
    }
}
