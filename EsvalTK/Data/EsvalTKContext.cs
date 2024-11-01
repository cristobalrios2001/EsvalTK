using EsvalTK.Models;
using Microsoft.EntityFrameworkCore;

namespace EsvalTK.Data
{
    public class EsvalTKContext : DbContext
    {
        public EsvalTKContext(DbContextOptions<EsvalTKContext> options) : base(options)
        {
        }

        public DbSet<Dispositivotk> Dispositivotk { get; set; }
        public DbSet<Medicion> Mediciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
        }
    }
}
