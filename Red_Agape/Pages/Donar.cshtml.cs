using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Red_Agape.Models;
using System.ComponentModel.DataAnnotations;

namespace Red_Agape.Pages
{
    public class DonacionInput
    {
        public TipoEntrega TipoEntrega { get; set; } = TipoEntrega.Recoleccion;
        public string? DireccionRecoger { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una zona.")]
        public int? ZonaId { get; set; }

        [Required(ErrorMessage = "Selecciona una organización.")]
        public int? OrgDestinoId { get; set; }
        public int? PeticionOrgId { get; set; }

        public List<ArticuloInput> Articulos { get; set; } = new();
    }

    public class ArticuloInput
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = "";
        public Condicion Condicion { get; set; }
        public IFormFile? Foto { get; set; } // todavía no guarda foto porq es mucho pedo
    }

    public class DonarModel : PageModel
    {
        private readonly List<string> _tipoEntrega = new List<string> { "Recoleccion", "EntregaDirecta" };
        private readonly List<string> _condicion = new List<string> { "Nuevo", "BuenEstado", "Usada" };
        public SelectList Zonas { get; set; } = null!;
        public SelectList Organizaciones { get; set; } = null!;
        private readonly RedAgapeContext _context;

        [BindProperty]
        public DonacionInput Input { get; set; } = new DonacionInput();

        public DonarModel(RedAgapeContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            await CargarListasAsync();
        }

        public async Task<IActionResult> OnPostDonacionAsync()
        {
            // Validaciones 
            if (Input.TipoEntrega == TipoEntrega.Recoleccion
                && string.IsNullOrWhiteSpace(Input.DireccionRecoger))
                ModelState.AddModelError("Input.DireccionRecoger", "Indica la dirección de recolección.");

            if (Input.Articulos.Count == 0)
                ModelState.AddModelError(string.Empty, "Agrega al menos un artículo.");

            // Los ids vienen del navegador: comprueba que existan
            if (Input.ZonaId is not null && !await _context.Zonas.AnyAsync(z => z.ZonaId == Input.ZonaId))
                ModelState.AddModelError("Input.ZonaId", "Zona no válida.");

            if (Input.OrgDestinoId is not null && !await _context.Organizaciones.AnyAsync(o => o.OrganizacionId == Input.OrgDestinoId))
                ModelState.AddModelError("Input.OrgDestinoId", "Organización no válida.");

            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return Page();
            }

            // Lógica de negocio: crear la donación y sus artículos, y guardar en la base de datos

            var donanteId = 1; // TEMPORAL: luego, el usuario autenticado
            var fechaActual = DateTime.UtcNow;
            var estadoInicial = await _context.EstadosDonacion // estado inicial donación
                .FirstAsync(e => e.Nombre == "Pendiente");

            var donacion = new Donacion // Modelo de EF para capturar: donación, artículos y primer historial de estado
            {
                // del usuario logueado
                DonanteId = donanteId,

                // del formulario
                ZonaId = Input.ZonaId!.Value,
                OrgDestinoId = Input.OrgDestinoId!.Value,
                PeticionOrgId = Input.PeticionOrgId,
                TipoEntrega = Input.TipoEntrega,
                DireccionRecoger = Input.TipoEntrega == TipoEntrega.Recoleccion ? Input.DireccionRecoger : null,
                Descripcion = Input.Descripcion,

                // del servidor
                EstadoDonacionId = estadoInicial.EstadoDonacionId,
                FechaRegistro = fechaActual,

                ArticulosDonados = Input.Articulos.Select(a => new ArticuloDonado
                {
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    Cantidad = a.Cantidad,
                    Condicion = a.Condicion
                }).ToList(),

                HistorialesEstado =
                {
                    new HistorialEstado
                    {
                        EstadoDonacionId = estadoInicial.EstadoDonacionId,
                        ResponsableId = donanteId,
                        Fecha = fechaActual,
                        Descripcion = "Donación registrada por el donante."
                    }
                }
            };

            _context.Donaciones.Add(donacion);
            await _context.SaveChangesAsync();

            return RedirectToPage("Gracias");
        }

        public async Task<IActionResult> OnPostArticulo()
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return Page();
            }

            return Page();
        }

        private async Task CargarListasAsync()
        {
            Zonas = new SelectList(
                await _context.Zonas.AsNoTracking().OrderBy(z => z.Nombre).ToListAsync(),
                "ZonaId", "Nombre");

            Organizaciones = new SelectList(
                await _context.Organizaciones.AsNoTracking().OrderBy(o => o.Nombre).ToListAsync(),
                "OrganizacionId", "Nombre");
        }
    }
}