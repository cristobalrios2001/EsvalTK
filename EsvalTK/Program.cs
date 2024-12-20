using DotNetEnv;
using EsvalTK.Data;
using EsvalTK.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuraci�n de variables de entorno
        ConfigureEnvironmentVariables();

        // Configuraci�n de servicios
        ConfigureServices(builder.Services);

        // Configuraci�n del host web
        ConfigureWebHost(builder.WebHost);

        var app = builder.Build();

        // Configuraci�n del pipeline de solicitudes HTTP
        ConfigureHttpPipeline(app);

        app.Run();
    }

    private static void ConfigureEnvironmentVariables()
    {
        // Cargar variables de entorno si no est�n definidas
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DB_USER")))
        {
            Env.Load();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Configurar cadena de conexi�n a la base de datos
        var connectionString = BuildConnectionString();

        // Configuraci�n del contexto de base de datos
        services.AddDbContext<DbContext>(options =>
            options.UseOracle(connectionString)
               .EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine));

        // Configuraci�n de Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EsvalTK API",
                Version = "v1",
                Description = "Documentaci�n de la API de EsvalTK"
            });
        });

        // Configuraci�n de controladores y servicios
        services.AddControllersWithViews();
        services.AddScoped<IDispositivotkService, DispositivotkService>();
        services.AddScoped<MedicionesService, MedicionesService>();
    }

    private static string BuildConnectionString()
    {
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var dbSource = Environment.GetEnvironmentVariable("DB_SOURCE");

        if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword) || string.IsNullOrEmpty(dbSource))
        {
            throw new InvalidOperationException("Faltan variables de entorno para configurar la base de datos.");
        }

        return $"User Id={dbUser};Password={dbPassword};Data Source={dbSource}";
    }

    private static void ConfigureWebHost(IWebHostBuilder webHost)
    {
        webHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(7121);  // Escucha en todas las interfaces en el puerto 7121
        });
    }

    private static void ConfigureHttpPipeline(WebApplication app)
    {
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
                c.RoutePrefix = "docs";
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthorization();

        // Configuraci�n de rutas
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Dispositivotks}/{action=Create}/{id?}");

        app.MapControllers();
        app.UseCors();
    }
}