using Newtonsoft.Json;

namespace ApiPaisesMVC_SQLServer.Services
{
    // Ejecuta una llamada a una API externa y devuelve null si falla por red,
    // tiempo de espera, respuesta HTTP de error (clave inválida, límite) o JSON inválido.
    public static class ApiSegura
    {
        public const string MensajeError =
            "No se pudo consultar la API. Verifica la clave y la conexión a internet.";

        public static async Task<List<T>?> IntentarAsync<T>(Func<Task<List<T>>> llamada)
        {
            try
            {
                return await llamada();
            }
            catch (Exception ex) when (ex is HttpRequestException
                                          or TaskCanceledException
                                          or JsonReaderException)
            {
                return null;
            }
        }
    }
}
