using EsvalTK.Models;
using EsvalTK.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EsvalTK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicionesController : ControllerBase
    {
        private readonly IMedicionService _medicionService;

        public MedicionesController(IMedicionService medicionService)
        {
            _medicionService = medicionService;
        }

        [HttpPost("RegisterWaterLevelMeasurement")]
        public async Task<IActionResult> RegistrarMedicion([FromBody] MedicionRequest model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _medicionService.RegistrarMedicionAsync(model);
                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return NotFound(ex.Message);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet("GetLatestMeasurementByDevice")]
        public async Task<IActionResult> ObtenerUltimaMedicionPorDispositivo()
        {
            return await _medicionService.ObtenerUltimasMedicionesAsync();
        }
    }
}