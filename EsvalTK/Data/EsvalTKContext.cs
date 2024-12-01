using EsvalTK.Models;
using Microsoft.EntityFrameworkCore;

namespace EsvalTK.Data
{
    public class EsvalTKContext : DbContext
    {
        // Constructor sin parámetros (requerido para Moq en pruebas)
        public EsvalTKContext()
        {
        }

        // Constructor con opciones para producción
        public EsvalTKContext(DbContextOptions<EsvalTKContext> options) : base(options)
        {
        }

        // Hacer las propiedades DbSet virtuales para permitir mocking
        public virtual DbSet<Dispositivotk> Dispositivotk { get; set; }
        public virtual DbSet<Medicion> Mediciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
