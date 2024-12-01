using System.Text.Json;
using System.Text;

namespace EsvalTK.Services
{
    public class ApiRequestHostedService : IHostedService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private Timer _timer;
        private const string ApiUrl = "https://localhost:7121/api/Mediciones/RegisterWaterLevelMeasurement";
        private const string DeviceId = "209290626"; // ID del dispositivo específico

        public ApiRequestHostedService()
        {
            _httpClient = new HttpClient();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Ejecutar cada 5 minutos
            _timer = new Timer(SendSimulatedMeasurements, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
            return Task.CompletedTask;
        }

        private async void SendSimulatedMeasurements(object state)
        {
            var random = new Random();
            int requestCount = random.Next(20, 26); // Generar un número entre 20 y 25

            for (int i = 0; i < requestCount; i++)
            {
                // Generar un nivel de agua simulado
                double simulatedWaterLevel = random.Next(0, 1000); // Rango de nivel de agua en mm o cm

                // Crear el objeto de solicitud
                var request = new
                {
                    IdDispositivo = DeviceId,
                    NivelAgua = simulatedWaterLevel
                };

                try
                {
                    var jsonContent = JsonSerializer.Serialize(request);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Realizar la solicitud HTTP POST
                    var response = await _httpClient.PostAsync(ApiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Solicitud {i + 1}/{requestCount} enviada con éxito: {jsonContent}");
                    }
                    else
                    {
                        Console.WriteLine($"Error en solicitud {i + 1}/{requestCount}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en solicitud {i + 1}/{requestCount}: {ex.Message}");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _httpClient.Dispose();
        }
    }
}
