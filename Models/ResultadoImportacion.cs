namespace ApiPaisesMVC_SQLServer.Models
{
    // Resultado de guardar un lote de datos de la API sin duplicados.
    // Recibidos: elementos que devolvió la API; Guardados: insertados como nuevos;
    // Existentes: omitidos por ya existir en la base de datos (o repetirse en el lote).
    public record ResultadoImportacion(int Recibidos, int Guardados, int Existentes)
    {
        // Devuelve el tipo de alerta (clave de TempData) y el mensaje para el usuario.
        public (string Tipo, string Mensaje) ParaUsuario()
        {
            if (Recibidos == 0)
                return ("Aviso", "Sin datos de la API. Verifica la clave y la conexión.");
            if (Guardados > 0)
                return ("Exito", $"{Guardados} nuevos guardados, {Existentes} ya existían.");
            if (Existentes == Recibidos)
                return ("Info", "Datos ya importados.");
            return ("Error", "No se pudo guardar ningún dato.");
        }
    }
}
