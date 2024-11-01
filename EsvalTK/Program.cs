using DotNetEnv;
using EsvalTK.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Estanques}/{action=Create}/{id?}");
app.MapControllers();

app.Run();
