using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Red_Agape.Models;

namespace Red_Agape.Pages
{
    public class IndexModel : PageModel
    {
        private readonly RedAgapeContext _context;
        public void OnGet()
        {
        }
    }
}
