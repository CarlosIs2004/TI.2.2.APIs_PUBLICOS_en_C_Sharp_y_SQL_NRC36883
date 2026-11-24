using ApiPaisesMVC_SQLServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    public class PaisRepository : IPaisRepository
    {
        private readonly AppDbContext _context;

        public PaisRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Pais?> ObtenerPorIdAsync(int id)
        {
            return await _context.Paises.FindAsync(id);
        }

        public async Task<List<Pais>> ListarAsync(string? nombre, string? region, string? capital)
        {
            IQueryable<Pais> consulta = _context.Paises;

            if (!string.IsNullOrWhiteSpace(nombre))
                consulta = consulta.Where(p => p.Nombre.Contains(nombre));
            if (!string.IsNullOrWhiteSpace(region))
                consulta = consulta.Where(p => p.Region == region);
            if (!string.IsNullOrWhiteSpace(capital))
                consulta = consulta.Where(p => p.Capital.Contains(capital));

            return await consulta.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<List<string>> ObtenerRegionesAsync()
        {
            return await _context.Paises
                .Where(p => p.Region != "")
                .Select(p => p.Region).Distinct().OrderBy(r => r).ToListAsync();
        }

        public async Task<int> ContarPorNombreAsync(string nombre)
        {
            return await _context.Paises.CountAsync(p => p.Nombre.Contains(nombre));
        }

        public async Task<bool> ExistePorNombreAsync(string nombre)
        {
            return await _context.Paises.AnyAsync(p => p.Nombre.Contains(nombre));
        }

        public async Task<bool> ExisteNombreExactoAsync(string nombre, int? excluirId = null)
        {
            if (excluirId == null)
                return await _context.Paises.AnyAsync(p => p.Nombre == nombre);
            return await _context.Paises.AnyAsync(p => p.Id != excluirId && p.Nombre == nombre);
        }

        public async Task AgregarAsync(Pais pais)
        {
            _context.Paises.Add(pais);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Pais pais)
        {
            _context.Paises.Update(pais);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Pais pais)
        {
            _context.Paises.Remove(pais);
            await _context.SaveChangesAsync();
        }

        public async Task VaciarAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Paises");
        }

        public async Task<ResultadoImportacion> GuardarNuevosAsync(IEnumerable<Pais> paises)
        {
            var lote = paises.ToList();
            var existentes = new HashSet<string>(
                await _context.Paises.Select(p => p.Nombre).ToListAsync(),
                StringComparer.OrdinalIgnoreCase);

            int guardados = 0;
            foreach (Pais pais in lote)
            {
                if (string.IsNullOrWhiteSpace(pais.Nombre) || !existentes.Add(pais.Nombre))
                    continue;
                _context.Paises.Add(pais);
                guardados++;
            }
            await _context.SaveChangesAsync();

            return new ResultadoImportacion(lote.Count, guardados, lote.Count - guardados);
        }
    }
}
