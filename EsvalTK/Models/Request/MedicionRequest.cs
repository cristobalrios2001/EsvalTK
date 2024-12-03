using System.ComponentModel.DataAnnotations;

namespace EsvalTK.Models
{
    public class MedicionRequest
    {
        [Required]
        public required string IdDispositivo { get; set; }

        [Required]
        public double NivelAgua { get; set; }
    }
}