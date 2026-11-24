using System.ComponentModel.DataAnnotations;

namespace ApiPaisesMVC_SQLServer.Models
{
    // Actividad Avanzada (sección 19 del documento): entidad para almacenar las
    // universidades obtenidas de la API pública http://universities.hipolabs.com.
    public class Universidad
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Dominio { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string SitioWeb { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Pais { get; set; } = string.Empty;
    }
}
