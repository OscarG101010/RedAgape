using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Red_Agape.Models;

namespace Red_Agape.Pages
{
    public class PeticionesModel : PageModel
    {
        private readonly RedAgapeContext _context;

        public PeticionesModel(RedAgapeContext context)
        {
            _context = context;
        }

        // ---------- Filtros (GET) ----------
        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }          // categoría general o sub

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }          // nombre de organización

        // ---------- Datos para la vista ----------
        public List<PeticionVM> Peticiones { get; set; } = new();
        public List<Categoria> CategoriasGenerales { get; set; } = new();

        public async Task OnGetAsync()
        {
            CategoriasGenerales = await _context.Categorias  // categorías padres (generales) para el filtro
                .Where(c => c.CategoriaPadreId == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Sólo peticiones vigentes de organizaciones activas
            var peticionesDb = await _context.PeticionesOrg
                .Include(p => p.Organizacion).ThenInclude(o => o.Zona)
                .Where(p => p.Activo
                         && p.FechaLimite >= DateTime.Now
                         && p.Organizacion.Activo)
                .ToListAsync();

            var ids = peticionesDb.Select(p => p.PeticionOrgId).ToList();

            // Sus necesidades (categoría + cantidad), con la categoría padre para agrupar
            var necesidadesDb = await _context.CategoriasPeticionOrg
                .Include(x => x.Categoria).ThenInclude(c => c.Padre)
                .Where(x => ids.Contains(x.PeticionOrgId))
                .ToListAsync();

            var porPeticion = necesidadesDb.ToLookup(x => x.PeticionOrgId);

            // Arma los ViewModels
            var lista = peticionesDb.Select(p => new PeticionVM
            {
                PeticionOrgId = p.PeticionOrgId,
                OrganizacionId = p.OrganizacionId,
                OrganizacionNombre = p.Organizacion.Nombre,
                Zona = p.Organizacion.Zona.Nombre,
                Municipio = p.Organizacion.Zona.Municipio,
                Descripcion = p.Descripcion,
                FechaLimite = p.FechaLimite,
                Necesidades = porPeticion[p.PeticionOrgId]
                    .Select(x =>
                    {
                        var general = x.Categoria.Padre ?? x.Categoria;
                        return new NecesidadVM
                        {
                            CategoriaId = x.CategoriaId,
                            Nombre = x.Categoria.Nombre,
                            GeneralId = general.CategoriaId,
                            GeneralNombre = general.Nombre,
                            Cantidad = x.CantidadSolicitada
                        };
                    })
                    .OrderBy(n => n.GeneralNombre).ThenBy(n => n.Nombre)
                    .ToList()
            });

            // Filtros
            if (CategoriaId.HasValue)
            {
                lista = lista.Where(p => p.Necesidades.Any(n =>
                    n.CategoriaId == CategoriaId || n.GeneralId == CategoriaId));
            }

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                lista = lista.Where(p => p.OrganizacionNombre
                    .Contains(Busqueda, StringComparison.OrdinalIgnoreCase));
            }

            Peticiones = lista.OrderBy(p => p.FechaLimite).ToList();
        }

        // ---------- ViewModels ----------
        public class PeticionVM
        {
            public int PeticionOrgId { get; set; }
            public int OrganizacionId { get; set; }
            public string OrganizacionNombre { get; set; } = string.Empty;
            public string Zona { get; set; } = string.Empty;
            public string Municipio { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public DateTime FechaLimite { get; set; }
            public int DiasRestantes => Math.Max(0, (FechaLimite.Date - DateTime.Today).Days);
            public List<NecesidadVM> Necesidades { get; set; } = new();
        }

        public class NecesidadVM
        {
            public int CategoriaId { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int GeneralId { get; set; }
            public string GeneralNombre { get; set; } = string.Empty;
            public int Cantidad { get; set; }
        }
    }
}