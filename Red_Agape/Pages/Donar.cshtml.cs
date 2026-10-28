using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Red_Agape.Models;
using Red_Agape.Data;

namespace Red_Agape.Pages
{ 
    public class DonarModel : PageModel
    {
        private readonly RedAgapeContext _context;

        public void OnGet() { }

        public IActionResult OnPost()
        {
            return Page();
        }
    }
}