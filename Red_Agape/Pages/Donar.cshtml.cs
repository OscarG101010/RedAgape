using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Red_Agape.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
    [Authorize]
    public class DonarModel : PageModel
    {
        private readonly List<string> _tipoEntrega = new List<string> { "Recoleccion", "EntregaDirecta" };
        private readonly List<string> _condicion = new List<string> { "Nuevo", "BuenEstado", "Usada" };
        public SelectList Zonas { get; set; } = null!;
        public SelectList Organizaciones { get; set; } = null!;
        private readonly RedAgapeContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _extPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

        [BindProperty]
        public DonacionInput Input { get; set; } = new DonacionInput();

        public DonarModel(RedAgapeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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

            for (var i = 0; i < Input.Articulos.Count; i++) // Validación Foto
            {
                var foto = Input.Articulos[i].Foto;
                if (foto is null || foto.Length == 0) continue;

                var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();

                if (!_extPermitidas.Contains(ext))
                    ModelState.AddModelError($"Input.Articulos[{i}].Foto", "Solo se permiten JPG, PNG o WEBP.");
                else if (foto.Length > MaxBytes)
                    ModelState.AddModelError($"Input.Articulos[{i}].Foto", "La foto no debe pasar de 5 MB.");
            }

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

            var articulos = new List<ArticuloDonado>();
            foreach (var a in Input.Articulos)
            {
                var rutaFoto = await GuardarFotoAsync(a.Foto);

                articulos.Add(new ArticuloDonado
                {
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    Cantidad = a.Cantidad,
                    Condicion = a.Condicion,
                    FotoUrl = rutaFoto
                });
            }

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

                ArticulosDonados = articulos,

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

            return RedirectToPage();
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

        private async Task<string?> GuardarFotoAsync(IFormFile? foto)
        {
            if (foto is null || foto.Length == 0) return null;

            var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
            var nombre = $"{Guid.NewGuid()}{ext}";

            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "donaciones");
            Directory.CreateDirectory(carpeta);

            var rutaFisica = Path.Combine(carpeta, nombre);
            await using var stream = new FileStream(rutaFisica, FileMode.Create);
            await foto.CopyToAsync(stream);

            return $"/uploads/donaciones/{nombre}";
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