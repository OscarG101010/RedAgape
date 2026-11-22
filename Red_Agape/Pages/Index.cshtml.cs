using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Microsoft.Data.SqlClient;
using Red_Agape.Models;
using Microsoft.AspNetCore.Identity;

namespace Red_Agape.Pages
{
    // Nombres de los roles. Deben ser idénticos a los de la columna Nombre de la tabla Rol.
    public static class RolesAgape
    {
        public const string Administrador = "Administrador";
        public const string Organizacion = "Organizacion";
        public const string Donante = "Donante";
        public const string Voluntario = "Voluntario";
    }

    // Modelo de página que procesa el inicio de sesión, el registro y el cierre de sesión.
    public class IndexModel : PageModel
    {
        /* 
         Datos admin: 
        Nombre: Administrador RedAgape
        Correo: admin@redagape.local
        Contraseña: AQAAAAIAAYagAAAAEAHnBvFqV2qGv8fzQs7uVXih4iWleotPMVIc0KxJZKlfHdKZtd4eio7/lEZgYKuDbA==
        */
        private readonly RedAgapeContext _context;

        private readonly IPasswordHasher<Usuario> _hasher;
        
        public IndexModel(RedAgapeContext context, IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // Si viene returnUrl, la página abre la cajita con un aviso.
        public void OnGet(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && !TempData.ContainsKey("MensajeAuth"))
            {
                TempData["MensajeAuth"] = "Inicia sesión para continuar.";
                TempData["AbrirCajita"] = true;
            }
        }

        // Procesa las credenciales enviadas desde el formulario de inicio de sesión.
        public async Task<IActionResult> OnPostLoginAsync(string CorreoOUsuario, string Password, string? returnUrl)
        {
            // Evita procesar solicitudes que no contienen los datos mínimos requeridos.
            if (string.IsNullOrWhiteSpace(CorreoOUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                return Error("Por favor completa todos los campos.", "login", returnUrl);
            }

            string dato = CorreoOUsuario.Trim().ToLower();

            // Busca el usuario y los roles del mismo en la base de datos por correo electrónico (sin importar mayúsculas/minúsculas).
            var usuario = await _context.Usuarios.Include(
                u => u.Roles).FirstOrDefaultAsync(
                u => u.Correo.ToLower() == dato || u.Nombre.ToLower() == dato);
            
            if (usuario == null || !usuario.Activo)
            {
                return Error("El correo o la contraseña son incorrectos.", "login", returnUrl);
            }

            // Verifica la contraseña.
            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, Password);
            
            if (resultado == PasswordVerificationResult.Failed)
            {
                return Error("El correo o la contraseña son incorrectos.", "login", returnUrl);
            }
            // valida si la contraseña necesita ser rehashada (actualizada) y la actualiza en la base de datos.
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.ContrasenaHash = _hasher.HashPassword(usuario, Password);
                await _context.SaveChangesAsync();
            }

            await CrearCookieSesionAsync(usuario); // Guarda la información del login en la cookie de autenticación.
            return RedirectToPage();
        }

        // Valida los datos del formulario y crea la sesión de un nuevo usuario.
        public async Task<IActionResult> OnPostRegistroAsync(
            string Nombre,
            string Correo, string Password, string ConfirmarPassword, string? returnUrl)
        {
            // Se repiten en el servidor las validaciones del navegador, porque esas se pueden saltar.
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Correo) ||
                string.IsNullOrWhiteSpace(Password))
            {
                return Error("Todos los campos son obligatorios.", "registro");
            }

            string username = Nombre.Trim();
            string correo = Correo.Trim().ToLower();

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]{3,20}$"))
            {
                return Error("El usuario debe tener de 3 a 20 letras, números, guion o punto.", "registro");
            }

            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                return Error("El correo no tiene un formato válido.", "registro");
            }

            if (Password != ConfirmarPassword)
            {
                return Error("Las contraseñas no coinciden.", "registro");
            }

            bool tieneMayuscula = Regex.IsMatch(Password, @"[A-ZÁÉÍÓÚÑ]");
            bool tieneEspecial = Regex.IsMatch(Password, @"[@#$%!^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]");

            if (Password.Length < 8 || !tieneMayuscula || !tieneEspecial)
            {
                return Error("La contraseña debe tener al menos 8 caracteres, una mayúscula y un carácter especial.", "registro");
            }

            // ¿Ya existe ese correo o ese usuario? AnyAsync genera un SELECT ... EXISTS.
            bool existe = await _context.Usuarios
                .AnyAsync(u => u.Correo == correo || u.Nombre == username);

            if (existe)
            {
                return Error("Ese correo o nombre de usuario ya está registrado.", "registro");
            }

            // Busca el rol Donante en la tabla Rol.
            var rolBase = await _context.Roles
                .FirstOrDefaultAsync(r => r.Nombre == RolesAgape.Donante);

            if (rolBase == null)
            {
                return Error("No se pudo asignar el rol. Contacta al administrador.", "registro");
            }

            var nuevo = new Usuario
            {
                Nombre = username,
                Correo = correo,
                Roles = new List<Rol> { rolBase }   // EF inserta la fila en UsuarioRol solo.
            };

            // El hash se genera DESPUÉS de crear el objeto; la contraseña real nunca se guarda.
            nuevo.ContrasenaHash = _hasher.HashPassword(nuevo, Password);

            try
            {
                _context.Usuarios.Add(nuevo);
                await _context.SaveChangesAsync();   // Equivale a los INSERT.
            }
            catch (DbUpdateException)
            {
                // Pasa si dos personas se registran con el mismo dato al mismo tiempo (índice único).
                return Error("Ese correo o nombre de usuario ya está registrado.", "registro");
            }

            await CrearCookieSesionAsync(nuevo);
            return VolverA(returnUrl); // Redirige a la página que se quería abrir antes del registro.
        }

        // Cierra la cookie de autenticación y devuelve al usuario a la página principal.
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage();
        }

        // Crea la cookie de autenticación con la información que se usará durante la sesión.
        private async Task CrearCookieSesionAsync(Usuario usuario)
        {
            var claims = new List<Claim> // Se guarda ID, Nombre y correo en la cookie
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo)
            };

            // Un usuario puede tener varios roles (UsuarioRol), así que va un claim por cada uno.
            foreach (var rol in usuario.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
            }

            // Crea la identidad de usuario con los claims y el esquema de autenticación de cookies.
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync( // Envia la cookie al navegador con la información de la sesión.
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    
        // Evita repetir las tres líneas de TempData en cada error.
        private IActionResult Error(string mensaje, string tab, string? returnUrl = null)
        {
            TempData["MensajeAuth"] = mensaje;
            TempData["AbrirCajita"] = true;
            TempData["TabActiva"] = tab;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            return RedirectToPage(new {returnUrl});
        }

        private IActionResult VolverA(string? returnUrl)
        {
            // IsLocalUrl evita que alguien arme un enlace que te mande a otro sitio después del login.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage();
        }
    }
}