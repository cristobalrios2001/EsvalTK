using DotNetEnv;
using EsvalTK.Data;
using EsvalTK.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno si no est�n definidas
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_USER")))
{
    Env.Load();
}

// Construir la cadena de conexi�n manualmente usando variables de entorno
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbSource = Environment.GetEnvironmentVariable("DB_SOURCE");

if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword) || string.IsNullOrEmpty(dbSource))
{
    throw new InvalidOperationException("Faltan variables de entorno para configurar la base de datos.");
}

var connectionString = $"User Id={dbUser};Password={dbPassword};Data Source={dbSource}";

// Configurar el contexto de base de datos con la cadena de conexi�n construida
builder.Services.AddDbContext<EsvalTKContext>(options =>
    options.UseOracle(connectionString)
           .EnableSensitiveDataLogging() // Permite ver informaci�n sensible en el log
           .LogTo(Console.WriteLine)); // Log a la consola

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EsvalTK API",
        Version = "v1",
        Description = "Documentaci�n de la API de EsvalTK utilizando Swagger"
    });
});

// Agregar controladores con vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    // Manejo de errores en producci�n
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Configuraci�n de Swagger en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EsvalTK API v1");
        c.RoutePrefix = "docs"; // Prefijo est�ndar para Swagger
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Configuraci�n de rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dispositivotks}/{action=Create}/{id?}");
app.MapControllers();

app.Run();
