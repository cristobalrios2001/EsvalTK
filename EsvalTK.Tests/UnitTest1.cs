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

namespace EsvalTK.Tests.Controllers
{
    public class DispositivotksControllerTests
    {
        private readonly Mock<IDispositivotkService> _mockService;
        private readonly DispositivotksController _controller;

        public DispositivotksControllerTests()
        {
            _mockService = new Mock<IDispositivotkService>();
            _controller = new DispositivotksController(_mockService.Object);
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_POST_ValidModel_RedirectsToCreateWithSuccessMessage()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "TEST001",
                NumeroEstanque = "EST001"
            };

            _mockService.Setup(s => s.CreateDispositivoAsync(model)).ReturnsAsync(true);

            // Configuración de TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["SuccessMessage"]);
        }


        [Fact]
        public async Task Create_POST_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "",
                NumeroEstanque = ""
            }; // Modelo inválido
            _controller.ModelState.AddModelError("IdDispositivo", "Required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
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
            Assert.Equal("No se encontró un dispositivo activo con el ID proporcionado.", value.Message); 
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
            Assert.Equal("Medición registrada con éxito.", ((dynamic)okResult.Value).Message);
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
            var value = Assert.IsType<MedicionResponse>(notFoundResult.Value); // Cast explícito a MedicionResponse
            Assert.Equal("No se encontraron mediciones.", value.Message); // Verificamos la comparación exacta
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