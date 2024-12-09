using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Glamping_Addventure.Models.Servicios.Implementaci�n;
using Microsoft.AspNetCore.Authentication;
using Glamping_Addventure3.Models.Servicios.Contrato;
using glamping_addventure3.Models;
using Glamping_Addventure3.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configura el contexto de la base de datos
builder.Services.AddDbContext<GlampingAddventure3Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BloggingDatabase")));

// Registra servicios personalizados
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // Aseg�rate de registrar el servicio de correo
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Configurar pol�ticas de autorizaci�n
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AccederDashboard", policy => policy.RequireClaim("Permission", "Acceder Dashboard"));
    options.AddPolicy("AccederUsuarios", policy => policy.RequireClaim("Permission", "Acceder Usuarios "));
    options.AddPolicy("AccederClientes", policy => policy.RequireClaim("Permission", "Acceder Clientes"));
    options.AddPolicy("AccederRoles", policy => policy.RequireClaim("Permission", "Acceder Roles"));
    options.AddPolicy("AccederMuebles", policy => policy.RequireClaim("Permission", "Acceder Muebles"));
   
    options.AddPolicy("AccederHabitaciones", policy => policy.RequireClaim("Permission", "Acceder Habitaciones"));
    options.AddPolicy("AccederTipoHabitaciones", policy => policy.RequireClaim("Permission", "Acceder Tipo de Habitaciones"));
    options.AddPolicy("AccederServicios", policy => policy.RequireClaim("Permission", "Acceder Servicios"));
    options.AddPolicy("AccederPaquetes", policy => policy.RequireClaim("Permission", "Acceder Paquetes"));
    options.AddPolicy("AccederReservas", policy => policy.RequireClaim("Permission", "Acceder Reservas"));
    options.AddPolicy("AccederAbonos", policy => policy.RequireClaim("Permission", "Acceder Abonos"));
});


// Configuraci�n de autenticaci�n
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Inicio/IniciarSesion";  // Redirige a esta p�gina si no est� autenticado
        options.AccessDeniedPath = "/Home/Index"; // Redirige a una p�gina de acceso denegado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true; // Permite renovar la sesi�n si el usuario est� activo
    });

// Configuraci�n de autorizaci�n para roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administradores", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("Recepcionistas", policy => policy.RequireRole("Recepcionista"));
});

// Configuraci�n de la sesi�n
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Cambia esto seg�n tus necesidades
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Necesario para que la sesi�n funcione
});

// Configuraci�n de filtros para controlar el almacenamiento en cach�
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new ResponseCacheAttribute
    {
        NoStore = true,
        Location = ResponseCacheLocation.None,
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5187")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

var app = builder.Build();

// Antes de app.UseAuthorization();
app.UseCors("PermitirLocalhost");
var defaultCulture = new CultureInfo("es-MX");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};

app.UseRequestLocalization(localizationOptions);


// A�adir el middleware para gestionar el tiempo de sesi�n
app.UseSession(); // Aseg�rate de usar la sesi�n antes de usar el middleware
app.UseMiddleware<SessionTimeoutMiddleware>(); // A�adir el middleware de sesi�n

// Configura el middleware para asegurar que los usuarios no autenticados sean redirigidos
app.UseAuthentication();
app.UseAuthorization();

// Configuraci�n de rutas para la aplicaci�n
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=IniciarSesion}/{id?}");

// Ruta para el controlador de recuperaci�n de contrase�a
app.MapControllerRoute(
    name: "recuperacion",
    pattern: "Recuperacion/{action=SolicitarRecuperacion}/{id?}",
    defaults: new { controller = "Recuperacion", action = "SolicitarRecuperacion" });

app.Run();

// Middleware para gestionar el tiempo de sesi�n
public class SessionTimeoutMiddleware
{
    private readonly RequestDelegate _next;

    public SessionTimeoutMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var lastActivityString = context.Session.GetString("LastActivity");
            if (lastActivityString != null)
            {
                var lastActivity = DateTime.Parse(lastActivityString);
                if (DateTime.UtcNow - lastActivity > TimeSpan.FromMinutes(20)) // Cambia el tiempo aqu� seg�n tu necesidad
                {
                    await Logout(context); // Cerrar sesi�n si la sesi�n ha expirado
                }
            }

            // Actualiza la �ltima actividad
            context.Session.SetString("LastActivity", DateTime.UtcNow.ToString());
        }

        await _next(context);
    }

    private async Task Logout(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Cerrar sesi�n
        context.Response.Redirect("/Inicio/IniciarSesion"); // Redirigir a la p�gina de inicio de sesi�n
    }
}