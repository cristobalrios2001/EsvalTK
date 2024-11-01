using EsvalTK.Data;
using EsvalTK.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EsvalTK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicionesController : ControllerBase
    {
        private readonly EsvalTKContext _context;

        public MedicionesController(EsvalTKContext context)
        {
            _context = context;
        }

        // Endpoint para recibir la medición desde el ESP32
        [HttpPost("RegisterWaterLevelMeasurement")]
        public async Task<IActionResult> RegistrarMedicion([FromBody] MedicionRequest model)
        {
            if (ModelState.IsValid)
            {

                var dispositivo = await _context.Dispositivotk.FirstOrDefaultAsync(d => d.Estado == 1 && d.IdDispositivo == model.IdDispositivo);

                if (dispositivo == null)
                {
                    return NotFound("No se encontró un dispositivo activo con el ID proporcionado.");
                }

                // Crear la nueva medición
                var medicion = new Medicion
                {
                    IdDispositivo = model.IdDispositivo,
                    Nivel = (long)model.NivelAgua,    // Conviertes el nivel de agua
                    Fecha = DateTime.Now,
                    IdRelacion = dispositivo.IdRelacion,
                };

                // Guardar la medición en la base de datos
                _context.Mediciones.Add(medicion);
                await _context.SaveChangesAsync();

                // Retornar un mensaje de éxito
                return Ok(new { Message = "Medición registrada con éxito." });
            }

            // Si el modelo no es válido, retornar un error de validación
            return BadRequest(ModelState);
        }


        [HttpGet("GetLatestMeasurementByDevice")]
        public async Task<IActionResult> ObtenerUltimaMedicionPorDispositivo()
        {
            var ultimasMediciones = await _context.Mediciones
                .Include(m => m.Dispositivotk) // Incluye la relación con Estanque
                .GroupBy(m => m.IdDispositivo)
                .Select(g => g.OrderByDescending(m => m.Fecha).FirstOrDefault())
                .ToListAsync();

            var resultado = ultimasMediciones.Select(m => new
            {
                m.IdDispositivo,
                NumeroEstanque = m.Dispositivotk?.NumeroEstanque,
                m.Nivel,
                Fecha = m.Fecha.Date,
                Hora = m.Fecha.TimeOfDay,
                 // Accede al NumeroEstanque
            }).ToList();

            // Verifica si se encontraron mediciones
            if (resultado == null || !resultado.Any())
            {
                return NotFound(new { Message = "No se encontraron mediciones." });
            }

            // Retorna las mediciones más recientes por dispositivo
            return Ok(resultado);
        }
    }

    // Modelo de solicitud para el POST
    public class MedicionRequest
    {
        [Required]
        public required string IdDispositivo { get; set; } // ID del dispositivo enviado por ESP32

        [Required]
        public double NivelAgua { get; set; } // Nivel de agua medido
    }
}
