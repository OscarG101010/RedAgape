namespace Red_Agape.Models
{
    public class Organizacion
    {
        public int OrganizacionId { get; set; }
        public int ZonaId { get; set; }
        public int EncargadoId { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty; // asignar valor define no nulleable (no salta advertencia)
        public string? Telefono { get; set; } // ? permite null

        public bool EsDonatariaAutorizada { get; set; } = false;
        public bool Verificado { get; set; } = false;
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        public Zona Zona { get; set; } = null!;
        public Usuario Encargado { get; set; } = null!;
    }
}