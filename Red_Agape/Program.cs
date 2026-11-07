<<<<<<< Updated upstream
using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
=======
using Microsoft.AspNetCore.Authentication.Cookies;
using Red_Agape.Models;
>>>>>>> Stashed changes

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RedAgapeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Configurar Autenticación por Cookies (sin Entity Framework, ideal para SqlClient)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RedAgape.Auth";
        options.LoginPath = "/Index";
        options.AccessDeniedPath = "/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// 2. Configurar Políticas basadas en Roles (escalable para añadir más roles después)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PoliticasAgape.SoloAdmin, policy =>
        policy.RequireRole(RolesAgape.Administrador));

    // Ejemplo escalable: hoy Admin, mañana Admin u Organizador
    options.AddPolicy(PoliticasAgape.GestionDonaciones, policy =>
        policy.RequireRole(RolesAgape.Administrador, RolesAgape.Organizador));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// IMPORTANTE: UseAuthentication siempre va antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();