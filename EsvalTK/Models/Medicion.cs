using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EsvalTK.Models
{
    [Table("MEDICION")] 
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

        [Column("ID_RELACION")]
        public required Guid IdRelacion { get; set; }

        [ForeignKey("IdRelacion")]
        public Dispositivotk? Dispositivotk { get; set; } 

        public Medicion()
        {
            Id = Guid.NewGuid(); 
        }
    }
}
