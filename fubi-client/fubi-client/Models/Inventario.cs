namespace fubi_client.Models
{
    public class Inventario
    {
        public int? id_inventario { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public int? cantidad { get; set; }
        public int? cantidad_prestada { get; set; }
        public string? ruta_imagen { get; set; }
    }
}
