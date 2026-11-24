using ApiPaisesMVC_SQLServer.Models;
using Newtonsoft.Json.Linq;

namespace ApiPaisesMVC_SQLServer.Services
{
    // Servicio para consumir la API pública de Universidades (Actividad Avanzada,
    // sección 19 del documento): http://universities.hipolabs.com/search
    // La API devuelve un arreglo plano de objetos y admite los filtros
    // 'country' (país) y 'name' (nombre).
    public class UniversidadApiService : IUniversidadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public UniversidadApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Universities:BaseUrl"]
                       ?? "http://universities.hipolabs.com/search";
        }

        // Importar las universidades de un país (ej.: ?country=Ecuador).
        public async Task<List<Universidad>> ObtenerPorPais(string pais)
        {
            string url = $"{_baseUrl}?country={Uri.EscapeDataString(pais.Trim())}";
            return Convertir(JArray.Parse(await _httpClient.GetStringAsync(url)));
        }

        // Buscar universidades por nombre, opcionalmente acotando por país.
        public async Task<List<Universidad>> ObtenerPorNombre(string nombre, string? pais = null)
        {
            string url = $"{_baseUrl}?name={Uri.EscapeDataString(nombre.Trim())}";
            if (!string.IsNullOrWhiteSpace(pais))
                url += $"&country={Uri.EscapeDataString(pais.Trim())}";
            return Convertir(JArray.Parse(await _httpClient.GetStringAsync(url)));
        }

        private static List<Universidad> Convertir(JArray items)
        {
            return items.Select(ConvertirUniversidad).ToList();
        }

        // Convierte un elemento JSON de la API en un objeto Universidad.
        private static Universidad ConvertirUniversidad(JToken item)
        {
            Universidad u = new Universidad();
            u.Nombre = item["name"]?.ToString() ?? "";
            u.Pais = item["country"]?.ToString() ?? "";

            // 'domains' y 'web_pages' son arreglos; tomamos el primer valor.
            var dominios = item["domains"] as JArray;
            u.Dominio = dominios != null && dominios.Count > 0
                ? dominios[0].ToString()
                : "Sin dominio";

            var paginas = item["web_pages"] as JArray;
            u.SitioWeb = paginas != null && paginas.Count > 0
                ? paginas[0].ToString()
                : "Sin sitio web";

            return u;
        }
    }
}
