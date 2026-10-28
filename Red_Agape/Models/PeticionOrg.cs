namespace Red_Agape.Models
{
    public class PeticionOrg
    {
        public int PeticionOrgId { get; set; }
        public int OrganizacionId { get; set; }

        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedad de navegación
        public Organizacion Organizacion { get; set; } = null!;
    }
}
