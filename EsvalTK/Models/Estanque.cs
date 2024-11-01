using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EsvalTK.Models
{
    [Table("DISPOSITIVOTK")] 
    public class Estanque
    {
        [Key]
        [Column("ID")]
        public required string Id { get; set; }

        [Column("ROTULOTK")]
        public required string NumeroEstanque { get; set; }

        // Relación uno a muchos con Medicion
        public ICollection<Medicion> Mediciones { get; set; }
    }
}
