namespace EsvalTK.Models.Responses
{
    public class MedicionResponse
    {
        public string Message { get; set; }
        public string IdDispositivo { get; set; }
        public long Nivel { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}