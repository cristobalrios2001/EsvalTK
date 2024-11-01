using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EsvalTK.Models
{
    [Table("MEDICION")] // Nombre de la tabla en la base de datos
    public class Medicion
    {
        [Key]
        [Column("ID")]
        public Guid Id { get; set; }

        [Column("ID_DISPOSITIVO")]
        public required string IdDispositivo { get; set; }

        [Column("NIVEL")]
        public long Nivel { get; set; }

        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        [ForeignKey("IdDispositivo")]
        public Estanque? Estanque { get; set; }

        public Medicion()
        {
            Id = Guid.NewGuid(); // Se genera un GUID único cuando se crea una instancia
        }
    }
}
