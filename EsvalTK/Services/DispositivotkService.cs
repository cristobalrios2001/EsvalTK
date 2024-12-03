using EsvalTK.Data;
using EsvalTK.Models;
using Microsoft.EntityFrameworkCore;

public interface IDispositivotkService
{
    Task<bool> CreateDispositivoAsync(DispositivotkViewModel model);
}

public class DispositivotkService : IDispositivotkService
{
    private readonly EsvalTKContext _context;

    public DispositivotkService(EsvalTKContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateDispositivoAsync(DispositivotkViewModel model)
    {
        if (model == null)
            return false;

        var dispositivo = new Dispositivotk
        {
            IdDispositivo = model.IdDispositivo,
            NumeroEstanque = model.NumeroEstanque
        };

        try
        {
            _context.Dispositivotk.Add(dispositivo);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            // Podrías añadir logging aquí
            return false;
        }
    }
}