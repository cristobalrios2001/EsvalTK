using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EsvalTK.Models
{
    [Table("DISPOSITIVOTK")]
    public class Dispositivotk
    {
        [Key]
        [Column("ID_RELACION")]
        public  Guid IdRelacion { get; set; } 

        [Column("ROTULOTK")]
        public required string NumeroEstanque { get; set; }

        [Column("ID_DISPOSITIVO")]
        public required string IdDispositivo { get; set; } 

        [Column("FECHA_INICIO")]
        public DateTime FechaInicio { get; set; }

        [Column("FECHA_FIN")]
        public DateTime? FechaFin { get; set; }

        [Column("ESTADO")]
        public int Estado { get; set; } = 1;

        // Relación uno a muchos con Medicion
        public ICollection<Medicion> Mediciones { get; set; } = new List<Medicion>(); 
        public Dispositivotk()
        {
            Mediciones = new List<Medicion>();
            IdRelacion = Guid.NewGuid();
            FechaInicio = DateTime.Now;
        }
    }
}
