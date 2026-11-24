namespace ApiPaisesMVC_SQLServer.Models
{
    // Modelo de vista para los reportes de la Actividad 5.
    public class ReportesViewModel
    {
        // Reporte 1: cantidad de países por región
        public List<RegionConteo> PaisesPorRegion { get; set; } = new();

        // Reporte 2: país más poblado
        public Pais? PaisMasPoblado { get; set; }

        // Reporte 3: promedio poblacional por región
        public List<RegionPromedio> PromedioPorRegion { get; set; } = new();
    }

    public class RegionConteo
    {
        public string Region { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class RegionPromedio
    {
        public string Region { get; set; } = string.Empty;
        public double PromedioPoblacion { get; set; }
    }
}
