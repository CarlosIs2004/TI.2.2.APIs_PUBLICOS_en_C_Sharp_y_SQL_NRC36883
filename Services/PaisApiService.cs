using ApiPaisesMVC_SQLServer.Models;
using Newtonsoft.Json.Linq;

namespace ApiPaisesMVC_SQLServer.Services
{
    // Servicio para consumir la API REST Countries v5 (sección 13 del documento).
    // La v5 vive en https://api.restcountries.com/countries/v5, requiere el header
    // Authorization: Bearer <api_key> (configurado en Program.cs) y envuelve los
    // resultados en { "data": { "objects": [...], "meta": {...} } } con paginación.
    public class PaisApiService : IPaisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        // Campos que solicitamos a la v5 para aligerar la respuesta (response_fields).
        private const string Campos =
            "names.common,names.official,capitals,region,subregion,population,area,currencies";

        // La v5 limita cada página a un máximo de 100 registros.
        private const int TamanioPagina = 100;

        public PaisApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["RestCountries:BaseUrl"]
                       ?? "https://api.restcountries.com/countries/v5";
        }

        // Importar todos los países. La v5 pagina los resultados (máx. 100 por página),
        // así que recorremos las páginas con offset hasta que meta.more sea false.
        public async Task<List<Pais>> ObtenerPaises()
        {
            var todos = new List<Pais>();
            int offset = 0;
            bool hayMas;

            do
            {
                string url = $"{_baseUrl}?limit={TamanioPagina}&offset={offset}" +
                             $"&response_fields={Campos}";
                JObject json = JObject.Parse(await _httpClient.GetStringAsync(url));

                if (json["data"]?["objects"] is JArray objetos)
                {
                    foreach (var pais in objetos)
                        todos.Add(ConvertirPais(pais));
                }

                hayMas = json["data"]?["meta"]?["more"]?.Value<bool>() ?? false;
                offset += TamanioPagina;
            }
            while (hayMas);

            return todos;
        }

        // Actividad 1: consumir la API para un país específico por nombre.
        // La v5 usa el parámetro 'q' para la búsqueda difusa por texto.
        public async Task<List<Pais>> ObtenerPaisPorNombre(string nombre)
        {
            string url = $"{_baseUrl}?q={Uri.EscapeDataString(nombre.Trim())}" +
                         $"&limit={TamanioPagina}&response_fields={Campos}";
            JObject json = JObject.Parse(await _httpClient.GetStringAsync(url));
            var objetos = json["data"]?["objects"] as JArray ?? new JArray();
            return objetos.Select(ConvertirPais).ToList();
        }

        // Convierte un elemento JSON de la API (v5) en un objeto Pais
        private static Pais ConvertirPais(JToken item)
        {
            Pais pais = new Pais();
            pais.Nombre = item["names"]?["common"]?.ToString() ?? "";
            pais.Region = item["region"]?.ToString() ?? "";
            pais.SubRegion = item["subregion"]?.ToString() ?? "";
            pais.Poblacion = item["population"]?.Value<long>() ?? 0;
            // En la v5 'area' es un objeto: { kilometers, miles }
            pais.Area = item["area"]?["kilometers"]?.Value<double>() ?? 0;

            // Capital: la v5 devuelve un arreglo de objetos { name, coordinates, ... }
            var capitales = item["capitals"] as JArray;
            pais.Capital = capitales != null && capitales.Count > 0
                ? capitales[0]?["name"]?.ToString() ?? "Sin capital"
                : "Sin capital";

            // Moneda: la v5 devuelve un arreglo de objetos { code, name, symbol }
            pais.Moneda = "N/D";
            if (item["currencies"] is JArray monedas && monedas.Count > 0)
            {
                pais.Moneda = monedas[0]?["name"]?.ToString() ?? "N/D";
            }
            return pais;
        }
    }
}
