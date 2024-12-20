using EsvalTK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EsvalTK.Controllers
{
    public class DispositivotksController : Controller
    {
        private readonly IDispositivotkService _dispositivotkService;
        private readonly ILogger<DispositivotksController> _logger;

        public DispositivotksController(
            IDispositivotkService dispositivotkService,
            ILogger<DispositivotksController> logger)
        {
            _dispositivotkService = dispositivotkService ?? throw new ArgumentNullException(nameof(dispositivotkService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DispositivotkViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var (success, message) = await _dispositivotkService.CreateDispositivoAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Create));
                }

                // Usando TempData para mostrar mensajes de error también
                TempData["ErrorMessage"] = message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la creación de la relación");
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al procesar la solicitud";
                return View(model);
            }
        }
    }
}