using ApiPaisesMVC_SQLServer.Models;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    // Consultas estadísticas de países, separadas del CRUD (principio de segregación de interfaces).
    public interface IReportePaisRepository
    {
        // Reporte 1: cantidad de países por región
        Task<List<RegionConteo>> PaisesPorRegionAsync();

        // Reporte 2: país más poblado
        Task<Pais?> PaisMasPobladoAsync();

        // Reporte 3: promedio poblacional por región
        Task<List<RegionPromedio>> PromedioPorRegionAsync();
    }
}
