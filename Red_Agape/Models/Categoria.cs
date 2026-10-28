namespace Red_Agape.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Propiedad de navegación para la relación muchos a muchos
        public ICollection<ArticuloDonado> ArticulosDonados { get; set; } = new List<ArticuloDonado>();
    }
}
