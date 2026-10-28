namespace Red_Agape.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty; // asignar valor define no nulleable (no salta advertencia)
        public string? Telefono { get; set; } // ? permite null
        public string ContrasenaHash { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedad de navegación para la relación muchos a muchos
        public ICollection<Rol> Roles { get; set; } = new List<Rol>(); // Investigar Lazy Loading y modificador de acceso virtual, no necesario EF Core
    }
}
