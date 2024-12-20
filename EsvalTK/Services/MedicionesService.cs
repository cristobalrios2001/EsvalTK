using EsvalTK.Data;
using EsvalTK.Models;
using Microsoft.EntityFrameworkCore;

namespace EsvalTK.Services
{
    public class MedicionesService : IMedicionesService
    {
        private readonly EsvalTKContext _context;

        public MedicionesService(EsvalTKContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarMedicionAsync(string idDispositivo, double nivelAgua)
        {
            var dispositivo = await _context.Dispositivotk.FirstOrDefaultAsync(d => d.Estado == 1 && d.IdDispositivo == idDispositivo);

            if (dispositivo == null)
            {
                return false;
            }

            var medicion = new Medicion
            {
                Nivel = (long)nivelAgua,
                Fecha = DateTime.Now,
                IdRelacion = dispositivo.IdRelacion
            };

            _context.Mediciones.Add(medicion);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<object>> ObtenerUltimaMedicionPorDispositivoAsync()
        {
            var ultimasMediciones = await _context.Mediciones
                .Include(m => m.Dispositivotk)
                .Where(m => m.Dispositivotk != null && m.Dispositivotk.Estado == 1)
                .GroupBy(m => m.IdRelacion)
                .Select(g => g.OrderByDescending(m => m.Fecha).FirstOrDefault())
                .ToListAsync();

            return ultimasMediciones.Select(m => new
            {
                IdRelacion = m.IdRelacion,
                NumeroEstanque = m.Dispositivotk?.NumeroEstanque,
                Nivel = m.Nivel,
                Fecha = m.Fecha.Date,
                Hora = m.Fecha.TimeOfDay
            }).ToList<object>();
        }
    }
}
