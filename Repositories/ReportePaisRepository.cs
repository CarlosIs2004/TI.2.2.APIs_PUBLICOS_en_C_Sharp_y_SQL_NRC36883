using ApiPaisesMVC_SQLServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    public class ReportePaisRepository : IReportePaisRepository
    {
        private readonly AppDbContext _context;

        public ReportePaisRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RegionConteo>> PaisesPorRegionAsync()
        {
            return await _context.Paises
                .Where(p => p.Region != "")
                .GroupBy(p => p.Region)
                .Select(g => new RegionConteo { Region = g.Key, Total = g.Count() })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }

        public async Task<Pais?> PaisMasPobladoAsync()
        {
            return await _context.Paises
                .OrderByDescending(p => p.Poblacion)
                .FirstOrDefaultAsync();
        }

        public async Task<List<RegionPromedio>> PromedioPorRegionAsync()
        {
            return await _context.Paises
                .Where(p => p.Region != "")
                .GroupBy(p => p.Region)
                .Select(g => new RegionPromedio
                {
                    Region = g.Key,
                    PromedioPoblacion = g.Average(p => (double)p.Poblacion)
                })
                .OrderByDescending(r => r.PromedioPoblacion)
                .ToListAsync();
        }
    }
}
