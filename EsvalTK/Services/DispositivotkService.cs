using EsvalTK.Data;
using EsvalTK.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IDispositivotkService
{
    Task<(bool success, string message)> CreateDispositivoAsync(DispositivotkViewModel model);
    Task<bool> ExisteRelacionActivaAsync(string idDispositivo, string numeroEstanque);
}

public class DispositivotkService : IDispositivotkService
{
    private readonly EsvalTKContext _context;
    private readonly ILogger<DispositivotkService> _logger;

    public DispositivotkService(EsvalTKContext context, ILogger<DispositivotkService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ExisteRelacionActivaAsync(string idDispositivo, string numeroEstanque)
    {
        var count = await _context.Dispositivotk
            .Where(d => d.Estado == 1 &&
                       (d.IdDispositivo == idDispositivo ||
                        d.NumeroEstanque == numeroEstanque))
            .CountAsync();

        return count > 0;
    }

    private async Task<Dispositivotk?> BuscarRelacionInactivaExactaAsync(string idDispositivo, string numeroEstanque)
    {
        return await _context.Dispositivotk
            .FirstOrDefaultAsync(d => d.Estado == 0 &&
                                    d.IdDispositivo == idDispositivo &&
                                    d.NumeroEstanque == numeroEstanque);
    }

    public async Task<(bool success, string message)> CreateDispositivoAsync(DispositivotkViewModel model)
    {
        try
        {
            if (model == null)
            {
                _logger.LogError("Intento de crear dispositivo con modelo nulo");
                return (false, "El modelo no puede ser nulo");
            }

            if (string.IsNullOrWhiteSpace(model.IdDispositivo) || string.IsNullOrWhiteSpace(model.NumeroEstanque))
            {
                return (false, "El ID del dispositivo y el número de estanque son requeridos");
            }

            var idDispositivoTrim = model.IdDispositivo.Trim();
            var numeroEstanqueTrim = model.NumeroEstanque.Trim();

           
            if (await ExisteRelacionActivaAsync(idDispositivoTrim, numeroEstanqueTrim))
            {
                _logger.LogWarning("Intento de crear relación duplicada. ID Dispositivo: {IdDispositivo}, Rotulo: {NumeroEstanque}",
                    idDispositivoTrim, numeroEstanqueTrim);
                return (false, "Ya existe una relación activa con el mismo dispositivo o número de estanque");
            }

            
            var relacionInactiva = await BuscarRelacionInactivaExactaAsync(idDispositivoTrim, numeroEstanqueTrim);

            if (relacionInactiva != null)
            {
                
                relacionInactiva.Estado = 1;
                relacionInactiva.FechaInicio = DateTime.Now;
                relacionInactiva.FechaFin = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Relación reactivada exitosamente: {IdDispositivo} - {NumeroEstanque}",
                    idDispositivoTrim, numeroEstanqueTrim);
                return (true, "La relación fue reactivada exitosamente");
            }

            
            var dispositivo = new Dispositivotk
            {
                IdDispositivo = idDispositivoTrim,
                NumeroEstanque = numeroEstanqueTrim,
                Estado = 1
            };

            _context.Dispositivotk.Add(dispositivo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nueva relación creada exitosamente: {IdDispositivo} - {NumeroEstanque}",
                idDispositivoTrim, numeroEstanqueTrim);
            return (true, "La relación fue creada exitosamente");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error al guardar la relación en la base de datos: {Message}", ex.Message);
            return (false, "Error al guardar la relación en la base de datos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear relación: {Message}", ex.Message);
            return (false, "Ocurrió un error inesperado al procesar la solicitud");
        }
    }
}