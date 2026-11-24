using ApiPaisesMVC_SQLServer.Models;
using ApiPaisesMVC_SQLServer.Repositories;
using ApiPaisesMVC_SQLServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiPaisesMVC_SQLServer.Controllers
{
    public class PaisController : Controller
    {
        private readonly IPaisRepository _repositorio;
        private readonly IReportePaisRepository _reportes;
        private readonly IPaisService _apiService;

        public PaisController(IPaisRepository repositorio, IReportePaisRepository reportes, IPaisService apiService)
        {
            _repositorio = repositorio;
            _reportes = reportes;
            _apiService = apiService;
        }

        // Vista principal
        public IActionResult Index()
        {
            return View();
        }

        // Importar todos los países desde la API (sin duplicar los ya existentes)
        public async Task<IActionResult> Importar()
        {
            List<Pais>? paises = await ApiSegura.IntentarAsync(() => _apiService.ObtenerPaises());
            if (paises == null)
            {
                TempData["Error"] = ApiSegura.MensajeError;
                return RedirectToAction("Lista");
            }
            ResultadoImportacion resultado = await _repositorio.GuardarNuevosAsync(paises);
            var (tipo, mensaje) = resultado.ParaUsuario();
            TempData[tipo] = mensaje;
            return RedirectToAction("Lista");
        }

        // Actividad 1: buscar un país por nombre. Primero en SQL Server; solo si no
        // hay coincidencias se consulta la API, se guarda y se vuelve a consultar la BD.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buscar(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return RedirectToAction("Lista");
            nombre = nombre.Trim();

            int encontrados = await _repositorio.ContarPorNombreAsync(nombre);
            if (encontrados > 0)
            {
                TempData["Info"] = $"{encontrados} resultado(s) obtenido(s) desde SQL Server";
                return RedirectToAction("Lista", new { nombre });
            }

            List<Pais>? desdeApi = await ApiSegura.IntentarAsync(() => _apiService.ObtenerPaisPorNombre(nombre));
            if (desdeApi == null)
            {
                TempData["Error"] = ApiSegura.MensajeError;
                return RedirectToAction("Lista");
            }
            await _repositorio.GuardarNuevosAsync(desdeApi);

            // El resultado mostrado siempre proviene de la base de datos.
            if (await _repositorio.ExistePorNombreAsync(nombre))
            {
                TempData["Exito"] = "El país se cargó desde la API y se guardó en SQL Server.";
                return RedirectToAction("Lista", new { nombre });
            }

            TempData["Aviso"] = $"No se encontró \"{nombre}\" en la base de datos ni en la API.";
            return RedirectToAction("Lista");
        }

        // Actividad 2 (Leer) + Actividad 3 (búsquedas) + Actividad 4 (filtro por región)
        public async Task<IActionResult> Lista(string? nombre, string? region, string? capital)
        {
            ViewBag.Regiones = await _repositorio.ObtenerRegionesAsync();
            ViewBag.Nombre = nombre;
            ViewBag.Region = region;
            ViewBag.Capital = capital;

            return View(await _repositorio.ListarAsync(nombre, region, capital));
        }

        // Actividad 2: Crear
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Pais pais)
        {
            pais.Nombre = pais.Nombre?.Trim() ?? "";
            if (ModelState.IsValid && await _repositorio.ExisteNombreExactoAsync(pais.Nombre))
                ModelState.AddModelError(nameof(Pais.Nombre), $"Ya existe un país llamado \"{pais.Nombre}\".");
            if (!ModelState.IsValid)
                return View(pais);

            await _repositorio.AgregarAsync(pais);
            TempData["Exito"] = "País agregado correctamente.";
            return RedirectToAction("Lista");
        }

        // Actividad 2: Actualizar
        public async Task<IActionResult> Editar(int id)
        {
            var pais = await _repositorio.ObtenerPorIdAsync(id);
            if (pais == null) return NotFound();
            return View(pais);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Pais pais)
        {
            pais.Nombre = pais.Nombre?.Trim() ?? "";
            if (ModelState.IsValid && await _repositorio.ExisteNombreExactoAsync(pais.Nombre, pais.Id))
                ModelState.AddModelError(nameof(Pais.Nombre), $"Ya existe un país llamado \"{pais.Nombre}\".");
            if (!ModelState.IsValid)
                return View(pais);

            var existente = await _repositorio.ObtenerPorIdAsync(pais.Id);
            if (existente == null) return NotFound();

            // Solo se copian los campos editables (evita overposting).
            existente.Nombre = pais.Nombre;
            existente.Capital = pais.Capital;
            existente.Region = pais.Region;
            existente.SubRegion = pais.SubRegion;
            existente.Poblacion = pais.Poblacion;
            existente.Area = pais.Area;
            existente.Moneda = pais.Moneda;
            await _repositorio.ActualizarAsync(existente);
            TempData["Exito"] = "País actualizado correctamente.";
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

        // Actividad 2: Eliminar
        public async Task<IActionResult> Eliminar(int id)
        {
            var pais = await _repositorio.ObtenerPorIdAsync(id);
            if (pais == null) return NotFound();
            return View(pais);
        }

        [HttpPost, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var pais = await _repositorio.ObtenerPorIdAsync(id);
            if (pais != null)
                await _repositorio.EliminarAsync(pais);
            return RedirectToAction("Lista");
        }

        // Actividad 5: reportes estadísticos
        public async Task<IActionResult> Reportes()
        {
            var modelo = new ReportesViewModel();

            // Reporte 1: cantidad de países por región
            modelo.PaisesPorRegion = await _reportes.PaisesPorRegionAsync();

            // Reporte 2: país más poblado
            modelo.PaisMasPoblado = await _reportes.PaisMasPobladoAsync();

            // Reporte 3: promedio poblacional por región
            modelo.PromedioPorRegion = await _reportes.PromedioPorRegionAsync();

            return View(modelo);
        }
    }
}
