using EsvalTK.Controllers;
using EsvalTK.Models;
using EsvalTK.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using EsvalTK.Data;
using Microsoft.EntityFrameworkCore;

public interface IMedicionService
{
    Task<MedicionResponse> RegistrarMedicionAsync(MedicionRequest model);
    Task<IActionResult> ObtenerUltimasMedicionesAsync();
}

public class MedicionService : IMedicionService
{
    private readonly EsvalTKContext _context;

    public MedicionService(EsvalTKContext context)
    {
        _context = context;
    }

    public async Task<MedicionResponse> RegistrarMedicionAsync(MedicionRequest model)
    {
        // Validar dispositivo
        var dispositivo = await _context.Dispositivotk
            .FirstOrDefaultAsync(d => d.Estado == 1 && d.IdDispositivo == model.IdDispositivo);

        if (dispositivo == null)
        {
            throw new Exception("No se encontró un dispositivo activo con el ID proporcionado.");
        }

        // Crear la nueva medición
        var medicion = new Medicion
        {
            Nivel = (long)model.NivelAgua,
            Fecha = DateTime.Now,
            IdRelacion = dispositivo.IdRelacion,
        };

        // Guardar la medición en la base de datos
        _context.Mediciones.Add(medicion);
        await _context.SaveChangesAsync();

        // Retornar respuesta
        return new MedicionResponse
        {
            Message = "Medición registrada con éxito.",
            IdDispositivo = model.IdDispositivo,
            Nivel = medicion.Nivel,
            FechaRegistro = medicion.Fecha
        };
    }

    public async Task<IActionResult> ObtenerUltimasMedicionesAsync()
    {
        var ultimasMediciones = await _context.Mediciones
            .Include(m => m.Dispositivotk)
            .GroupBy(m => m.IdRelacion)
            .Select(g => g.OrderByDescending(m => m.Fecha).FirstOrDefault())
            .ToListAsync();

        var resultado = ultimasMediciones.Select(m => new
        {
            IdRelacion = m.IdRelacion,
            NumeroEstanque = m.Dispositivotk?.NumeroEstanque,
            Nivel = m.Nivel,
            Fecha = m.Fecha.Date,
            Hora = m.Fecha.TimeOfDay,
        }).ToList();

        if (resultado == null || !resultado.Any())
        {
            return new NotFoundObjectResult(new { Message = "No se encontraron mediciones." });
        }

        return new OkObjectResult(resultado);
    }
}