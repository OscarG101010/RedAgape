namespace Red_Agape.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int? CategoriaPadreId { get; set; } // Nullable para permitir categorías sin padre

        public Categoria? Padre { get; set; }
        public ICollection<Categoria> Subcategorias { get; set; } = new List<Categoria>();
        // Propiedad de navegación para la relación muchos a muchos
        public ICollection<ArticuloDonado> ArticulosDonados { get; set; } = new List<ArticuloDonado>();
        public ICollection<Organizacion> Organizaciones { get; set; } = new List<Organizacion>();
    }
}
