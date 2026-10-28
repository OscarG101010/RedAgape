namespace Red_Agape.Models
{
    public class Rol
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Propiedad de navegación para la relación muchos a muchos
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
