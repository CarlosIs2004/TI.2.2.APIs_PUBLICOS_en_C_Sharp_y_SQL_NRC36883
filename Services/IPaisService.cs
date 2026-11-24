using ApiPaisesMVC_SQLServer.Models;

namespace ApiPaisesMVC_SQLServer.Services
{
    // Contrato para consumir la API externa de países (inversión de dependencias).
    public interface IPaisService
    {
        Task<List<Pais>> ObtenerPaises();
        Task<List<Pais>> ObtenerPaisPorNombre(string nombre);
    }
}
