using EsvalTK.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EsvalTK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicionesController : ControllerBase
    {
        private readonly MedicionesService _medicionesService;

        public MedicionesController(MedicionesService medicionesService)
        {
            _medicionesService = medicionesService;
        }

        [HttpPost("RegisterWaterLevelMeasurement")]
        public async Task<IActionResult> RegistrarMedicion([FromBody] MedicionRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _medicionesService.RegistrarMedicionAsync(model.IdDispositivo, model.NivelAgua);

            if (!resultado)
            {
                return NotFound(new { Message = "No se encontró un dispositivo activo con el ID proporcionado." });
            }

            return Ok(new { Message = "Medición registrada con éxito." });
        }

        [HttpGet("GetLatestMeasurementByDevice")]
        public async Task<IActionResult> ObtenerUltimaMedicionPorDispositivo()
        {
            var resultado = await _medicionesService.ObtenerUltimaMedicionPorDispositivoAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound(new { Message = "No se encontraron mediciones." });
            }

            return Ok(resultado);
        }
    }

    // Modelo de solicitud para el POST
    public class MedicionRequest
    {
        [Required]
        public required string IdDispositivo { get; set; }

        [Required]
        public double NivelAgua { get; set; }
    }
}
