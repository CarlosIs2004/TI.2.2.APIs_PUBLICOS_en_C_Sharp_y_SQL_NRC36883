using ApiPaisesMVC_SQLServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    public class UniversidadRepository : IUniversidadRepository
    {
        private readonly AppDbContext _context;

        public UniversidadRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Universidad?> ObtenerPorIdAsync(int id)
        {
            return await _context.Universidades.FindAsync(id);
        }

        public async Task<List<Universidad>> ListarAsync(string? nombre, string? pais)
        {
            IQueryable<Universidad> consulta = _context.Universidades;

            if (!string.IsNullOrWhiteSpace(nombre))
                consulta = consulta.Where(u => u.Nombre.Contains(nombre));
            if (!string.IsNullOrWhiteSpace(pais))
                consulta = consulta.Where(u => u.Pais == pais);

            return await consulta.OrderBy(u => u.Nombre).ToListAsync();
        }

        public async Task<List<string>> ObtenerPaisesAsync()
        {
            return await _context.Universidades
                .Where(u => u.Pais != "")
                .Select(u => u.Pais).Distinct().OrderBy(p => p).ToListAsync();
        }

        // Consulta por nombre (contiene) y, si se indica, por país exacto.
        private IQueryable<Universidad> ConsultarPorNombre(string nombre, string? pais)
        {
            IQueryable<Universidad> consulta = _context.Universidades
                .Where(u => u.Nombre.Contains(nombre));
            if (pais != null)
                consulta = consulta.Where(u => u.Pais == pais);
            return consulta;
        }

        public async Task<int> ContarPorNombreAsync(string nombre, string? pais)
        {
            return await ConsultarPorNombre(nombre, pais).CountAsync();
        }

        public async Task<bool> ExistePorNombreAsync(string nombre, string? pais)
        {
            return await ConsultarPorNombre(nombre, pais).AnyAsync();
        }

        public async Task<bool> ExisteExactaAsync(string nombre, string pais, int? excluirId = null)
        {
            if (excluirId == null)
                return await _context.Universidades
                    .AnyAsync(u => u.Nombre == nombre && u.Pais == pais);
            return await _context.Universidades
                .AnyAsync(u => u.Id != excluirId && u.Nombre == nombre && u.Pais == pais);
        }

        public async Task AgregarAsync(Universidad universidad)
        {
            _context.Universidades.Add(universidad);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Universidad universidad)
        {
            _context.Universidades.Update(universidad);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Universidad universidad)
        {
            _context.Universidades.Remove(universidad);
            await _context.SaveChangesAsync();
        }

        public async Task VaciarAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Universidades");
        }

        public async Task<ResultadoImportacion> GuardarNuevasAsync(IEnumerable<Universidad> universidades)
        {
            var lote = universidades.ToList();
            var existentes = new HashSet<string>(
                (await _context.Universidades.Select(u => new { u.Nombre, u.Pais }).ToListAsync())
                    .Select(u => Clave(u.Nombre, u.Pais)),
                StringComparer.OrdinalIgnoreCase);

            int guardados = 0;
            foreach (Universidad u in lote)
            {
                if (string.IsNullOrWhiteSpace(u.Nombre) || !existentes.Add(Clave(u.Nombre, u.Pais)))
                    continue;
                _context.Universidades.Add(u);
                guardados++;
            }
            await _context.SaveChangesAsync();

            return new ResultadoImportacion(lote.Count, guardados, lote.Count - guardados);
        }

        private static string Clave(string nombre, string pais) => $"{nombre}\u0001{pais}";
    }
}
