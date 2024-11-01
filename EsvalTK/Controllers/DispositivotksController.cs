using EsvalTK.Data;
using EsvalTK.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;



namespace EsvalTK.Controllers
{
    public class DispositivotksController : Controller
    {
        private readonly EsvalTKContext _context;

        public DispositivotksController(EsvalTKContext context)
        {
            _context = context;
        }

        // Acción para mostrar el formulario de ingreso de datos
        public IActionResult Create()
        {
            return View();
        }

        // Acción para procesar el envío del formulario
        [HttpPost]
        public async Task<IActionResult> Create(DispositivotkViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dispositivo = new Dispositivotk
                {
                    IdDispositivo = model.IdDispositivo,  
                    NumeroEstanque = model.NumeroEstanque
                };

                _context.Dispositivotk.Add(dispositivo);
                await _context.SaveChangesAsync();

                // Si el registro es exitoso, establecer el mensaje en TempData
                TempData["SuccessMessage"] = "El registro del dispositivo y estanque fue realizado con éxito.";

                // Redirigir de nuevo a la misma vista para que el mensaje se muestre
                return RedirectToAction("Create");
            }

            // Si el modelo no es válido, mostrar de nuevo el formulario con los errores
            return View(model);
        }
    }
}
