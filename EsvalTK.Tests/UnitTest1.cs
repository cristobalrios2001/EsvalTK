using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using EsvalTK.Controllers;
using EsvalTK.Models;
using EsvalTK.Data;
using Microsoft.AspNetCore.Http;
using EsvalTK.Models.Responses;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using EsvalTK.Services;
using Microsoft.Extensions.Logging;

namespace EsvalTK.Tests.Controllers
{
    public class DispositivotksControllerTests
    {
        private readonly Mock<IDispositivotkService> _mockService;
        private readonly Mock<ILogger<DispositivotksController>> _mockLogger;
        private readonly DispositivotksController _controller;
        private readonly TempDataDictionary _tempData;

        public DispositivotksControllerTests()
        {
            _mockService = new Mock<IDispositivotkService>();
            _mockLogger = new Mock<ILogger<DispositivotksController>>();
            _controller = new DispositivotksController(_mockService.Object, _mockLogger.Object);

            // Configuraci�n de TempData que ser� usada en m�ltiples tests
            _tempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
            _controller.TempData = _tempData;
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_POST_ValidModel_Success_RedirectsToCreateWithSuccessMessage()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "TEST001",
                NumeroEstanque = "EST001"
            };

            _mockService.Setup(s => s.CreateDispositivoAsync(model))
                .ReturnsAsync((true, "La relaci�n fue creada exitosamente"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("La relaci�n fue creada exitosamente", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task Create_POST_ValidModel_Failure_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "TEST001",
                NumeroEstanque = "EST001"
            };

            _mockService.Setup(s => s.CreateDispositivoAsync(model))
                .ReturnsAsync((false, "Ya existe una relaci�n activa"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Ya existe una relaci�n activa", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_POST_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "",
                NumeroEstanque = ""
            };
            _controller.ModelState.AddModelError("IdDispositivo", "Required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_POST_ServiceThrowsException_ReturnsViewWithErrorMessage()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "TEST001",
                NumeroEstanque = "EST001"
            };

            _mockService.Setup(s => s.CreateDispositivoAsync(model))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("Ocurri� un error inesperado al procesar la solicitud",
                _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task Create_POST_ValidModel_Reactivation_RedirectsToCreateWithSuccessMessage()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "TEST001",
                NumeroEstanque = "EST001"
            };

            _mockService.Setup(s => s.CreateDispositivoAsync(model))
                .ReturnsAsync((true, "La relaci�n fue reactivada exitosamente"));

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("La relaci�n fue reactivada exitosamente",
                _controller.TempData["SuccessMessage"]);
        }
    }

    public class MedicionesControllerTests
    {
        private readonly Mock<IMedicionesService> _mockService;
        private readonly MedicionesController _controller;

        public MedicionesControllerTests()
        {
            _mockService = new Mock<IMedicionesService>();
            _controller = new MedicionesController(_mockService.Object);
        }

        [Fact]
        public async Task RegisterWaterLevelMeasurement_DeviceNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new EsvalTK.Controllers.MedicionRequest
            {
                IdDispositivo = "NONEXISTENT",
                NivelAgua = 100.5
            };

            _mockService.Setup(s => s.RegistrarMedicionAsync(request.IdDispositivo, request.NivelAgua))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.RegistrarMedicion(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = Assert.IsType<MedicionResponse>(notFoundResult.Value); 
            Assert.Equal("No se encontr� un dispositivo activo con el ID proporcionado.", value.Message); 
        }


        [Fact]
        public async Task RegisterWaterLevelMeasurement_ValidDevice_ReturnsOk()
        {
            // Arrange
            var request = new EsvalTK.Controllers.MedicionRequest
            {
                IdDispositivo = "EXISTENT",
                NivelAgua = 75.5
            };

            _mockService.Setup(s => s.RegistrarMedicionAsync(request.IdDispositivo, request.NivelAgua))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.RegistrarMedicion(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Medici�n registrada con �xito.", ((dynamic)okResult.Value).Message);
        }

        [Fact]
        public async Task GetLatestMeasurementByDevice_NoMeasurementsFound_ReturnsNotFound()
        {
            // Arrange
            _mockService.Setup(s => s.ObtenerUltimaMedicionPorDispositivoAsync())
                        .ReturnsAsync(new List<object>());

            // Act
            var result = await _controller.ObtenerUltimaMedicionPorDispositivo();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var value = Assert.IsType<MedicionResponse>(notFoundResult.Value); // Cast expl�cito a MedicionResponse
            Assert.Equal("No se encontraron mediciones.", value.Message); // Verificamos la comparaci�n exacta
        }



        [Fact]
        public async Task GetLatestMeasurementByDevice_MeasurementsFound_ReturnsOk()
        {
            // Arrange
            var measurements = new List<object>
            {
                new { IdRelacion = Guid.NewGuid(), NumeroEstanque = "123ABC", Nivel = 75, Fecha = DateTime.Now.Date, Hora = DateTime.Now.TimeOfDay }
            };

            _mockService.Setup(s => s.ObtenerUltimaMedicionPorDispositivoAsync())
                        .ReturnsAsync(measurements);

            // Act
            var result = await _controller.ObtenerUltimaMedicionPorDispositivo();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(measurements, okResult.Value);
        }
    }

}