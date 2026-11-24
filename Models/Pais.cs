using System.ComponentModel.DataAnnotations;

namespace ApiPaisesMVC_SQLServer.Models
{
    public class Pais
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Capital { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Region { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string SubRegion { get; set; } = string.Empty;

        public long Poblacion { get; set; }
        public double Area { get; set; }

        [Required, MaxLength(100)]
        public string Moneda { get; set; } = string.Empty;
    }
}
