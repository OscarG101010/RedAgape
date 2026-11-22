using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Red_Agape.Models;

namespace Red_Agape.Pages
{
    [Authorize]
    public class MisDonacionesModel : PageModel
    {
        private readonly RedAgapeContext _db;
        public MisDonacionesModel(RedAgapeContext db) => _db = db;

        [BindProperty(SupportsGet = true)]
        public int? EstadoId { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        public List<EstadoDonacion> Estados { get; set; } = new();
        public List<DonacionVm> Donaciones { get; set; } = new();

        private int UsuarioId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task OnGetAsync()
        {
            Estados = await _db.EstadosDonacion.AsNoTracking().OrderBy(e => e.EstadoDonacionId).ToListAsync();

            var query = _db.Donaciones.AsNoTracking()
                .Where(d => d.DonanteId == UsuarioId);

            if (EstadoId.HasValue)
                query = query.Where(d => d.EstadoDonacionId == EstadoId.Value);

            Donaciones = await query
                .OrderByDescending(d => d.FechaRegistro)
                .Select(d => new DonacionVm
                {
                    DonacionId = d.DonacionId,
                    FechaRegistro = d.FechaRegistro,
                    Organizacion = d.OrgDestino.Nombre,
                    DireccionOrganizacion = d.OrgDestino.Direccion,
                    EstadoId = d.EstadoDonacionId,
                    Estado = d.EstadoDonacion.Nombre,
                    TipoEntrega = d.TipoEntrega,
                    DireccionRecoger = d.DireccionRecoger,
                    Descripcion = d.Descripcion,

                    Articulos = d.ArticulosDonados.Select(a => new ArticuloVm
                    {
                        Nombre = a.Nombre,
                        Cantidad = a.Cantidad,
                        Condicion = a.Condicion,
                        FotoUrl = a.FotoUrl,
                        Categorias = a.Categorias.Select(c => c.Nombre).ToList()
                    }).ToList(),

                    Historial = d.HistorialesEstado
                        .OrderByDescending(h => h.Fecha)
                        .Select(h => new HistorialVm
                        {
                            Fecha = h.Fecha,
                            Estado = h.EstadoDonacion.Nombre, // requiere la navegación en HistorialEstado
                            Descripcion = h.Descripcion
                        }).ToList()
                })
                .AsSplitQuery() // evita el producto cartesiano artículos × historial
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelarAsync(int id)
        {
            // Filtra por DonanteId: nadie cancela donaciones ajenas cambiando el id
            var donacion = await _db.Donaciones
                .Include(d => d.EstadoDonacion)
                .FirstOrDefaultAsync(d => d.DonacionId == id && d.DonanteId == UsuarioId);

            if (donacion == null) return NotFound();

            if (donacion.EstadoDonacion.Nombre != "Pendiente")
            {
                Mensaje = "Solo puedes cancelar donaciones pendientes.";
                return RedirectToPage();
            }

            var cancelada = await _db.EstadosDonacion.FirstAsync(e => e.Nombre == "Cancelada");

            donacion.EstadoDonacion = cancelada; // asignar la navegación evita FK y objeto desincronizados
            donacion.HistorialesEstado.Add(new HistorialEstado
            {
                EstadoDonacionId = cancelada.EstadoDonacionId,
                ResponsableId = UsuarioId,
                Fecha = DateTime.UtcNow,
                Descripcion = "Donación cancelada por el donante."
            });

            await _db.SaveChangesAsync(); // un solo SaveChanges = estado + historial atómicos

            Mensaje = "Donación cancelada.";
            return RedirectToPage();
        }

        // ---------- ViewModels ----------
        public class DonacionVm
        {
            public int DonacionId { get; set; }
            public DateTime FechaRegistro { get; set; }
            public string Organizacion { get; set; } = "";
            public string DireccionOrganizacion { get; set; } = "";
            public int EstadoId { get; set; }
            public string Estado { get; set; } = "";
            public TipoEntrega TipoEntrega { get; set; }
            public string? DireccionRecoger { get; set; }
            public string? Descripcion { get; set; }
            public List<ArticuloVm> Articulos { get; set; } = new();
            public List<HistorialVm> Historial { get; set; } = new();
        }

        public class ArticuloVm
        {
            public string Nombre { get; set; } = "";
            public int Cantidad { get; set; }
            public Condicion Condicion { get; set; }
            public string? FotoUrl { get; set; }
            public List<string> Categorias { get; set; } = new();
        }

        public class HistorialVm
        {
            public DateTime Fecha { get; set; }
            public string Estado { get; set; } = "";
            public string Descripcion { get; set; } = "";
        }
    }
}