namespace Red_Agape.Models
{
    public class HistorialEstado
    {
        public int HistorialEstadoId { get; set; }
        public int DonacionId { get; set; }
        public int? ResponsableId { get; set; }
        public int EstadoDonacionId { get; set; }

        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Propiedades de navegación
        public Donacion Donacion { get; set; } = null!;
        public Usuario? Responsable { get; set; }
        public EstadoDonacion EstadoDonacion { get; set; } = null!;
    }
}
