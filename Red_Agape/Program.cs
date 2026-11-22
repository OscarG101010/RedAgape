using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Red_Agape.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RedAgapeContext>(options => //ASP.Net Core permite este método para inyección de dependencias
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Configurar Autenticación por Cookies (sin Entity Framework, ideal para SqlClient)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RedAgape.Auth";
        options.LoginPath = "/Index";
        options.AccessDeniedPath = "/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

// 2. Configurar Políticas basadas en Roles (escalable para añadir más roles después)
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>(); // Cuando se pida hasher se agrega el servicio

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

// IMPORTANTE: UseAuthentication siempre va antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();