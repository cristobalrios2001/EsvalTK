using EsvalTK.Models;

namespace EsvalTK.Services
{
    public interface IMedicionesService
    {
        Task<bool> RegistrarMedicionAsync(string idDispositivo, double nivelAgua);
        Task<List<object>> ObtenerUltimaMedicionPorDispositivoAsync();
    }
}
