using System.ComponentModel.DataAnnotations;

namespace EsvalTK.Models
{
    public class EstanqueViewModel
    {
        [Required(ErrorMessage = "El ID del dispositivo es obligatorio")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "El ID del dispositivo solo puede contener números.")]
        public required string IdDispositivo { get; set; }

        [Required(ErrorMessage = "El número del estanque es obligatorio")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "El número del estanque solo puede contener letras y números.")]
        public required string NumeroEstanque { get; set; }
    }
}
