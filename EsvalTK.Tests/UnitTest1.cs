using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using EsvalTK.Controllers;
using EsvalTK.Models;
using EsvalTK.Data;
using Microsoft.AspNetCore.Http;
using EsvalTK.Models.Responses;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EsvalTK.Tests.Controllers
{
    public class DispositivotksControllerTests
    {
        private readonly Mock<EsvalTKContext> _mockContext;
        private readonly DispositivotksController _controller;

        public DispositivotksControllerTests()
        {
            _mockContext = new Mock<EsvalTKContext>();
            _controller = new DispositivotksController(_mockContext.Object);
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

            var mockSet = new Mock<DbSet<Dispositivotk>>();
            _mockContext.Setup(c => c.Dispositivotk).Returns(mockSet.Object);

            // Configuración de TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.NotNull(_controller.TempData["SuccessMessage"]); // Ahora debería pasar
        }


        [Fact]
        public async Task Create_POST_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new DispositivotkViewModel
            {
                IdDispositivo = "",
                NumeroEstanque = ""
            }; // Modelo inválido pero inicializado
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
        private readonly EsvalTKContext _context;
        private readonly MedicionesController _controller;

        public MedicionesControllerTests()
        {
            // Configura el contexto con la base de datos en memoria
            var options = new DbContextOptionsBuilder<EsvalTKContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new EsvalTKContext(options);

            // Se pasa el contexto al controlador
            _controller = new MedicionesController(_context);
        }

        [Fact]
        public async Task RegisterWaterLevelMeasurement_DeviceNotFound_ReturnsNotFound()
        {
            // Arrange: Asegurarse de que no haya dispositivos en la base de datos
            _context.Dispositivotk.RemoveRange(_context.Dispositivotk);
            await _context.SaveChangesAsync();

            var request = new MedicionRequest
            {
                IdDispositivo = "NONEXISTENT",
                NivelAgua = 100.5
            };

            // Act: Llamar al método del controlador
            var result = await _controller.RegistrarMedicion(request);

            // Assert: Verificar que el resultado sea NotFound
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No se encontró un dispositivo activo con el ID proporcionado.", notFoundResult.Value);
        }

        [Fact]
        public async Task RegisterWaterLevelMeasurement_ValidDevice_ReturnsOk()
        {
            // Arrange: Agregar un dispositivo válido a la base de datos
            var dispositivo = new Dispositivotk
            {
                IdDispositivo = "EXISTENT",
                Estado = 1,
                IdRelacion = Guid.NewGuid(),
                NumeroEstanque= "123ABC"
            };

            _context.Dispositivotk.Add(dispositivo);
            await _context.SaveChangesAsync();

            var request = new MedicionRequest
            {
                IdDispositivo = "EXISTENT",
                NivelAgua = 75.5
            };

            // Act: Llamar al método del controlador
            var result = await _controller.RegistrarMedicion(request);

            // Assert: Verificar que el resultado sea Ok
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<MedicionResponse>(okResult.Value);

            Assert.Equal("Medición registrada con éxito.", response.Message);
            Assert.Equal(request.IdDispositivo, response.IdDispositivo);
            Assert.Equal((long)request.NivelAgua, response.Nivel);
        }
    }

}