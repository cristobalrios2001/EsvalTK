using EsvalTK.Models;
using Microsoft.AspNetCore.Mvc;

namespace EsvalTK.Controllers
{
    public class DispositivotksController : Controller
    {
        private readonly IDispositivotkService _dispositivotkService;

        public DispositivotksController(IDispositivotkService dispositivotkService)
        {
            _dispositivotkService = dispositivotkService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DispositivotkViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _dispositivotkService.CreateDispositivoAsync(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "El registro del dispositivo y estanque fue realizado con éxito.";
                    return RedirectToAction("Create");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar el dispositivo.");
                }
            }

            return View(model);
        }
    }
}