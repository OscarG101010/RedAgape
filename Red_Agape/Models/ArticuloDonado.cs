namespace Red_Agape.Models
{
    public enum Condicion
    {
        Nuevo,
        BuenEstado,
        Usada
    }
    public class ArticuloDonado
    {
        public int ArticuloDonadoId { get; set; }
        public int DonacionId { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; } = 1;
        public string Descripcion { get; set; } = string.Empty;
        public Condicion Condicion { get; set; }
        public string? FotoUrl { get; set; }

        // Propiedad de navegación
        public Donacion Donacion { get; set; } = null!;
        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>(); // Las listas genericas se usan ya sea para relaciones de muchos a muchos o de uno a muchos del lado del uno
    }
}
