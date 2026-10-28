namespace Red_Agape.Models
{
    public class CategoriaPeticionOrg // Si necesita clase intermedia ya que tiene un atributo propio
    {
        // Llave primaria compuesta y llaves foráneas
        public int PeticionOrgId { get; set; }
        public int CategoriaId { get; set; }

        // Atributo propio de la relación
        public int CantidadSolicitada { get; set; } = 1;

        // Propiedades de navegación
        public PeticionOrg PeticionOrg { get; set; } = null!;
        public Categoria Categoria { get; set; } = null!;
    }
}
