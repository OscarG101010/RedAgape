namespace Red_Agape.Models
{
    public enum TipoEntrega
    {
        Recoleccion,
        EntregaDirecta
    }

    public class Donacion
    {
        public int DonacionId { get; set; }
        public int DonanteId { get; set; }
        public int ZonaId { get; set; }
        public int OrgDestinoId { get; set; }
        public int? PeticionOrgId { get; set; }
        public int EstadoDonacionId { get; set; }

        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Recoleccion;
        public string? DireccionRecoger { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Descripcion { get; set; }

        // Propiedades de navegación
        public Usuario Donante { get; set; } = null!;
        public Zona Zona { get; set; } = null!;
        public Organizacion OrgDestino { get; set; } = null!;
        public PeticionOrg? PeticionOrg { get; set; }
        public EstadoDonacion EstadoDonacion { get; set; } = null!;

        public ICollection<ArticuloDonado> ArticulosDonados { get; set; } = new List<ArticuloDonado>();
        public ICollection<HistorialEstado> HistorialesEstado { get; set; } = new List<HistorialEstado>();
    }
}
