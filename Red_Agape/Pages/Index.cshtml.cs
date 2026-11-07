using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
<<<<<<< Updated upstream
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
=======
using Microsoft.Data.SqlClient;
>>>>>>> Stashed changes
using Red_Agape.Models;

namespace Red_Agape.Pages
{
    // Modelo de página que procesa el inicio de sesión, el registro y el cierre de sesión.
    public class IndexModel : PageModel
    {
<<<<<<< Updated upstream
        private readonly RedAgapeContext _context;
        public void OnGet()
        {
=======
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            // La página principal no necesita cargar datos adicionales al abrirse, todavia no es neceario.
        }

        // Procesa las credenciales enviadas desde el formulario de inicio de sesión.
        public async Task<IActionResult> OnPostLoginAsync(string CorreoOUsuario, string Password)
        {
            // Evita procesar solicitudes que no contienen los datos mínimos requeridos.
            if (string.IsNullOrWhiteSpace(CorreoOUsuario) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["MensajeAuth"] = "Por favor completa todos los campos.";
                TempData["AbrirCajita"] = true;
                return RedirectToPage();
            }

            // =========================================================================
            // CONEXIÓN SQL SERVER CON MICROSOFT.DATA.SQLCLIENT (Descomentar al conectar BD)
            // =========================================================================
            /*
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string sql = @"SELECT NombreUsuario, Nombres, Apellidos, Correo, Rol 
                           FROM Usuarios 
                           WHERE (Correo = @Dato OR NombreUsuario = @Dato) 
                             AND PasswordHash = @Password AND Activo = 1";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Dato", CorreoOUsuario.Trim());
            cmd.Parameters.AddWithValue("@Password", Password);

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                string usernameBd = reader["NombreUsuario"].ToString()!;
                string nombreCompletoBd = $"{reader["Nombres"]} {reader["Apellidos"]}".Trim();
                string correoBd = reader["Correo"].ToString()!;
                string rolBd = reader["Rol"].ToString()!;

                await CrearCookieSesionAsync(usernameBd, nombreCompletoBd, correoBd, rolBd);
                return RedirectToPage();
            }
            else
            {
                TempData["MensajeAuth"] = "Usuario/correo o contraseña incorrectos.";
                TempData["AbrirCajita"] = true;
                return RedirectToPage();
            }
            */

            // Modo demo mientras la base de datos no está conectada: el texto "admin"
            // determina si la sesión se crea con rol Administrador o UsuarioFinal.
            string rolDemo = CorreoOUsuario.Contains("admin", StringComparison.OrdinalIgnoreCase)
                ? RolesAgape.Administrador
                : RolesAgape.UsuarioFinal;

            string usernameDemo = CorreoOUsuario.Contains('@')
                ? CorreoOUsuario.Split('@')[0]
                : CorreoOUsuario.Trim();

            await CrearCookieSesionAsync(usernameDemo, usernameDemo, CorreoOUsuario, rolDemo);
            return RedirectToPage();
        }

        // Valida los datos del formulario y crea la sesión de un nuevo usuario.
        public async Task<IActionResult> OnPostRegistroAsync(
            string Nombres,
            string Apellidos,
            string NombreUsuario,
            string Correo,
            string Password,
            string ConfirmarPassword)
        {
            // Comprueba que los campos obligatorios hayan sido diligenciados.
            if (string.IsNullOrWhiteSpace(Nombres) || string.IsNullOrWhiteSpace(Apellidos) ||
                string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Correo) ||
                string.IsNullOrWhiteSpace(Password))
            {
                TempData["MensajeAuth"] = "Todos los campos son obligatorios.";
                TempData["AbrirCajita"] = true;
                TempData["TabActiva"] = "registro";
                return RedirectToPage();
            }

            if (Password != ConfirmarPassword)
            {
                // La contraseña y su confirmación deben coincidir antes de continuar.
                TempData["MensajeAuth"] = "Las contraseñas no coinciden.";
                TempData["AbrirCajita"] = true;
                TempData["TabActiva"] = "registro";
                return RedirectToPage();
            }

            // Valida que la contraseña tenga al menos 8 caracteres, una mayúscula y un carácter especial.
            bool tieneMayuscula = Regex.IsMatch(Password, @"[A-ZÁÉÍÓÚÑ]");
            bool tieneEspecial = Regex.IsMatch(Password, @"[@#$%!^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]");

            if (Password.Length < 8 || !tieneMayuscula || !tieneEspecial)
            {
                TempData["MensajeAuth"] = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un carácter especial (ej. @).";
                TempData["AbrirCajita"] = true;
                TempData["TabActiva"] = "registro";
                return RedirectToPage();
            }

            // Todo usuario registrado desde la web comienza con el rol UsuarioFinal.
            string rolAsignado = RolesAgape.UsuarioFinal;
            string nombreCompleto = $"{Nombres.Trim()} {Apellidos.Trim()}";
            string usernameLimpio = NombreUsuario.Trim();

            // =========================================================================
            // INSERT EN SQL SERVER CON MICROSOFT.DATA.SQLCLIENT (Descomentar al conectar BD)
            // =========================================================================
            /*
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            string sqlInsert = @"INSERT INTO Usuarios (NombreUsuario, Nombres, Apellidos, Correo, PasswordHash, Rol) 
                                 VALUES (@NombreUsuario, @Nombres, @Apellidos, @Correo, @Password, @Rol)";

            using SqlCommand cmd = new SqlCommand(sqlInsert, conn);
            cmd.Parameters.AddWithValue("@NombreUsuario", usernameLimpio);
            cmd.Parameters.AddWithValue("@Nombres", Nombres.Trim());
            cmd.Parameters.AddWithValue("@Apellidos", Apellidos.Trim());
            cmd.Parameters.AddWithValue("@Correo", Correo.Trim());
            cmd.Parameters.AddWithValue("@Password", Password);
            cmd.Parameters.AddWithValue("@Rol", rolAsignado);
            await cmd.ExecuteNonQueryAsync();
            */

            await CrearCookieSesionAsync(usernameLimpio, nombreCompleto, Correo.Trim(), rolAsignado);
            return RedirectToPage();
        }

        // Cierra la cookie de autenticación y devuelve al usuario a la página principal.
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage();
        }

        // Crea la cookie de autenticación con la información que se usará durante la sesión.
        private async Task CrearCookieSesionAsync(string nombreUsuario, string nombreCompleto, string correo, string rol)
        {
            // Las claims permiten consultar posteriormente el nombre, correo y rol del usuario.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim("NombreCompleto", nombreCompleto),
                new Claim(ClaimTypes.Email, correo),
                new Claim(ClaimTypes.Role, rol)
            };

            // Vincula las claims al esquema de autenticación basado en cookies.
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // Registra la identidad en el contexto HTTP para mantener la sesión iniciada.
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
>>>>>>> Stashed changes
        }
    }
}