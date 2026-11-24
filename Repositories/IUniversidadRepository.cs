using ApiPaisesMVC_SQLServer.Models;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    // Acceso a datos de Universidad: solo CRUD y consultas (principio de segregación de interfaces).
    public interface IUniversidadRepository
    {
        Task<Universidad?> ObtenerPorIdAsync(int id);
        Task<List<Universidad>> ListarAsync(string? nombre, string? pais);

        // Países distintos (no vacíos) presentes en la tabla, ordenados.
        Task<List<string>> ObtenerPaisesAsync();

        // Búsqueda por nombre (contiene) y, si se indica, por país exacto.
        Task<int> ContarPorNombreAsync(string nombre, string? pais);
        Task<bool> ExistePorNombreAsync(string nombre, string? pais);

        // Coincidencia exacta de Nombre + País; excluirId ignora el registro que se edita.
        Task<bool> ExisteExactaAsync(string nombre, string pais, int? excluirId = null);

        Task AgregarAsync(Universidad universidad);
        Task ActualizarAsync(Universidad universidad);
        Task EliminarAsync(Universidad universidad);
        Task VaciarAsync();

        // Guarda las universidades omitiendo las ya existentes (por Nombre + País, sin
        // distinguir mayúsculas) y los repetidos dentro del mismo lote.
        Task<ResultadoImportacion> GuardarNuevasAsync(IEnumerable<Universidad> universidades);
    }
}
