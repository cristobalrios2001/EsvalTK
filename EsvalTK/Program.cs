using DotNetEnv;
using EsvalTK.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno
Env.Load();

// Construir la cadena de conexión manualmente usando variables de entorno
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var dbSource = Environment.GetEnvironmentVariable("DB_SOURCE");

var connectionString = $"User Id={dbUser};Password={dbPassword};Data Source={dbSource}";

// Configurar el contexto de base de datos con la cadena de conexión construida
builder.Services.AddDbContext<EsvalTKContext>(options =>
    options.UseOracle(connectionString)
           .EnableSensitiveDataLogging() // Permite ver la información de los parámetros
           .LogTo(Console.WriteLine)); // Log a la consola

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EsvalTK API",
        Version = "v1",
        Description = "Documentación de la API de EsvalTK utilizando Swagger",
    });

    // Configuración de seguridad para JWT (opcional)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el campo siguiente (sin 'Bearer')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Activar Swagger solo en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EsvalTK API v1");
        c.RoutePrefix = "swagger"; // Hace que Swagger esté disponible en la raíz
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dispositivotks}/{action=Create}/{id?}");
app.MapControllers();

app.Run();
