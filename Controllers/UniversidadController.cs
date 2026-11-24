using ApiPaisesMVC_SQLServer.Models;
using ApiPaisesMVC_SQLServer.Repositories;
using ApiPaisesMVC_SQLServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiPaisesMVC_SQLServer.Controllers
{
    // Actividad Avanzada (sección 19): consumo de la API de Universidades y
    // almacenamiento en SQL Server mediante la entidad Universidad.
    public class UniversidadController : Controller
    {
        private readonly IUniversidadRepository _repositorio;
        private readonly IUniversidadService _apiService;

        public UniversidadController(IUniversidadRepository repositorio, IUniversidadService apiService)
        {
            _repositorio = repositorio;
            _apiService = apiService;
        }

        // Vista principal
        public IActionResult Index()
        {
            return View();
        }

        // Importar universidades por país (el documento usa ?country=Ecuador),
        // sin duplicar las ya existentes.
        public async Task<IActionResult> Importar(string? pais)
        {
            string consultaPais = string.IsNullOrWhiteSpace(pais) ? "Ecuador" : pais;
            List<Universidad>? universidades = await ApiSegura.IntentarAsync(() => _apiService.ObtenerPorPais(consultaPais));
            if (universidades == null)
            {
                TempData["Error"] = ApiSegura.MensajeError;
                return RedirectToAction("Lista");
            }
            ResultadoImportacion resultado = await _repositorio.GuardarNuevasAsync(universidades);
            var (tipo, mensaje) = resultado.ParaUsuario();
            TempData[tipo] = mensaje;
            return RedirectToAction("Lista");
        }

        // Buscar por nombre (y opcionalmente país). Primero en SQL Server; solo si no
        // hay coincidencias se consulta la API, se guarda y se vuelve a consultar la BD.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buscar(string? nombre, string? pais)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return RedirectToAction("Lista");
            nombre = nombre.Trim();
            pais = string.IsNullOrWhiteSpace(pais) ? null : pais.Trim();

            int encontrados = await _repositorio.ContarPorNombreAsync(nombre, pais);
            if (encontrados > 0)
            {
                TempData["Info"] = $"{encontrados} resultado(s) obtenido(s) desde SQL Server";
                return RedirectToAction("Lista", new { nombre, pais });
            }

            List<Universidad>? desdeApi = await ApiSegura.IntentarAsync(() => _apiService.ObtenerPorNombre(nombre, pais));
            if (desdeApi == null)
            {
                TempData["Error"] = ApiSegura.MensajeError;
                return RedirectToAction("Lista");
            }
            await _repositorio.GuardarNuevasAsync(desdeApi);

            // El resultado mostrado siempre proviene de la base de datos.
            if (await _repositorio.ExistePorNombreAsync(nombre, pais))
            {
                TempData["Exito"] = "La universidad se cargó desde la API y se guardó en SQL Server.";
                return RedirectToAction("Lista", new { nombre, pais });
            }

            TempData["Aviso"] = $"No se encontró \"{nombre}\" en la base de datos ni en la API.";
            return RedirectToAction("Lista");
        }

        // Leer + búsquedas + filtro por país.
        public async Task<IActionResult> Lista(string? nombre, string? pais)
        {
            ViewBag.Paises = await _repositorio.ObtenerPaisesAsync();
            ViewBag.Nombre = nombre;
            ViewBag.Pais = pais;

            return View(await _repositorio.ListarAsync(nombre, pais));
        }

        // Crear
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Universidad universidad)
        {
            universidad.Nombre = universidad.Nombre?.Trim() ?? "";
            universidad.Pais = universidad.Pais?.Trim() ?? "";
            if (ModelState.IsValid && await _repositorio.ExisteExactaAsync(universidad.Nombre, universidad.Pais))
                ModelState.AddModelError(nameof(Universidad.Nombre),
                    $"Ya existe una universidad llamada \"{universidad.Nombre}\" en {universidad.Pais}.");
            if (!ModelState.IsValid)
                return View(universidad);

            await _repositorio.AgregarAsync(universidad);
            TempData["Exito"] = "Universidad agregada correctamente.";
            return RedirectToAction("Lista");
        }

        // Actualizar
        public async Task<IActionResult> Editar(int id)
        {
            var universidad = await _repositorio.ObtenerPorIdAsync(id);
            if (universidad == null) return NotFound();
            return View(universidad);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Universidad universidad)
        {
            universidad.Nombre = universidad.Nombre?.Trim() ?? "";
            universidad.Pais = universidad.Pais?.Trim() ?? "";
            if (ModelState.IsValid &&
                await _repositorio.ExisteExactaAsync(universidad.Nombre, universidad.Pais, universidad.Id))
                ModelState.AddModelError(nameof(Universidad.Nombre),
                    $"Ya existe una universidad llamada \"{universidad.Nombre}\" en {universidad.Pais}.");
            if (!ModelState.IsValid)
                return View(universidad);

            var existente = await _repositorio.ObtenerPorIdAsync(universidad.Id);
            if (existente == null) return NotFound();

            // Solo se copian los campos editables (evita overposting).
            existente.Nombre = universidad.Nombre;
            existente.Dominio = universidad.Dominio;
            existente.SitioWeb = universidad.SitioWeb;
            existente.Pais = universidad.Pais;
            await _repositorio.ActualizarAsync(existente);
            TempData["Exito"] = "Universidad actualizada correctamente.";
            return RedirectToAction("Lista");
        }

        // Vaciar la tabla: exige escribir la palabra "eliminar".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vaciar(string? confirmacion)
        {
            if (!string.Equals(confirmacion?.Trim(), "eliminar", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Texto incorrecto. No se eliminó nada.";
                return RedirectToAction("Lista");
            }

            await _repositorio.VaciarAsync();
            TempData["Exito"] = "Base de datos vaciada.";
            return RedirectToAction("Lista");
        }

        // Eliminar
        public async Task<IActionResult> Eliminar(int id)
        {
            var universidad = await _repositorio.ObtenerPorIdAsync(id);
            if (universidad == null) return NotFound();
            return View(universidad);
        }

        [HttpPost, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var universidad = await _repositorio.ObtenerPorIdAsync(id);
            if (universidad != null)
                await _repositorio.EliminarAsync(universidad);
            return RedirectToAction("Lista");
        }
    }
}
