using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Red_Agape.Models;

namespace Red_Agape.Pages
{
    public class DonarModel : PageModel
    {
        [BindProperty]
        public Donacion Input { get; set; } = new();

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (Input.TipoEntrega == TipoEntrega.RecoleccionDomicilio
                && string.IsNullOrWhiteSpace(Input.Direccion))
                ModelState.AddModelError("Input.Direccion", "La dirección es obligatoria para recolección.");

            if (!ModelState.IsValid) return Page();

            // Aquí irá el guardado más adelante
            TempData["Donante"] = Input.NombreDonante;
            return RedirectToPage("Gracias");   // patrón Post-Redirect-Get
        }
    }
}