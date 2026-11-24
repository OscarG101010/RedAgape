using Microsoft.EntityFrameworkCore;
using Red_Agape.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Red_Agape.Models;
using Microsoft.Extensions.FileProviders;

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
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;//CookieSecurePolicy.Always; // Solo se envía la cookie en conexiones HTTPS
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

// 2. Configurar Políticas basadas en Roles (escalable para añadir más roles después)
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>(); // Cuando se pida hasher se agrega el servicio
builder.Services.AddScoped<Red_Agape.Services.CorreoService>();


var app = builder.Build();

app.Lifetime.ApplicationStopping.Register(() =>
    Console.WriteLine("### ApplicationStopping:\n" + Environment.StackTrace));

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    Console.WriteLine("### ProcessExit");

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    Console.WriteLine("### UnhandledException: " + e.ExceptionObject);

TaskScheduler.UnobservedTaskException += (_, e) =>
    Console.WriteLine("### UnobservedTask: " + e.Exception);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var uploadsRoot = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "RedAgapeUploads"));
Directory.CreateDirectory(uploadsRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = "/uploads"
});

// IMPORTANTE: UseAuthentication siempre va antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();