using ApiPaisesMVC_SQLServer.Models;

namespace ApiPaisesMVC_SQLServer.Services
{
    // Contrato para consumir la API externa de universidades.
    public interface IUniversidadService
    {
        Task<List<Universidad>> ObtenerPorPais(string pais);
        Task<List<Universidad>> ObtenerPorNombre(string nombre, string? pais = null);
    }
}
