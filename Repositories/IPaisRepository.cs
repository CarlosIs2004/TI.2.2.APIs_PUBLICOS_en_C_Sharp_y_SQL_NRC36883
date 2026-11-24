using ApiPaisesMVC_SQLServer.Models;

namespace ApiPaisesMVC_SQLServer.Repositories
{
    // Acceso a datos de Pais: solo CRUD y consultas (principio de segregación de interfaces).
    public interface IPaisRepository
    {
        Task<Pais?> ObtenerPorIdAsync(int id);
        Task<List<Pais>> ListarAsync(string? nombre, string? region, string? capital);
        Task<List<string>> ObtenerRegionesAsync();

        // Cantidad de países cuyo nombre contiene el texto indicado.
        Task<int> ContarPorNombreAsync(string nombre);
        Task<bool> ExistePorNombreAsync(string nombre);

        // Coincidencia exacta de nombre; excluirId permite ignorar el registro que se edita.
        Task<bool> ExisteNombreExactoAsync(string nombre, int? excluirId = null);

        Task AgregarAsync(Pais pais);
        Task ActualizarAsync(Pais pais);
        Task EliminarAsync(Pais pais);
        Task VaciarAsync();

        // Guarda los países omitiendo los ya existentes (por Nombre, sin distinguir
        // mayúsculas) y los repetidos dentro del mismo lote.
        Task<ResultadoImportacion> GuardarNuevosAsync(IEnumerable<Pais> paises);
    }
}
