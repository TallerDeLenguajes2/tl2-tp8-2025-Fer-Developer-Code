using Microsoft.AspNetCore.Mvc;
using TP7.PresupuestoModel;
using TP7.PresupuestosRepositorySpace;
using TP7.ProductoRepositorySpace;
using TP7.ProductosModel;

namespace TP8.Controllers;

public class PresupuestosController : Controller
{
    // 1. Referencia al repositorio de presupuestos
    private readonly PresupuestosRepository _presupuestoRepository;
    private readonly ProductoRepository _productoRepository;

    // 2. Constructor: Inicializamos el repositorio
    public PresupuestosController()
    {
        _presupuestoRepository = new PresupuestosRepository();
        _productoRepository = new ProductoRepository();
    }

    // 3. Acción Index: Responde a GET /Presupuestos
    public IActionResult Index()
    {
        // 4. Pedimos la lista COMPLETA de presupuestos al repo
        //    (El repo se encarga de traer los detalles de c/u)
        List<Presupuesto> presupuestos = _presupuestoRepository.GetAll();

        // 5. Pasamos la lista a la vista "Index.cshtml"
        return View(presupuestos);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        try
        {
            // 1. Buscamos el presupuesto. El repositorio ya se encarga
            //    de traer todos sus detalles (productos) gracias al JOIN.
            var presupuesto = _presupuestoRepository.GetById(id);

            // 2. Verificamos si se encontró
            if (presupuesto == null)
            {
                return NotFound("No se encontró el presupuesto solicitado.");
            }

            // 3. Pasamos el objeto Presupuesto (con su lista de detalles) a la Vista.

            // 1. Obtenemos la lista de TODOS los productos disponibles
            var productosDisponibles = _productoRepository.GetProducts();

            // 2. Usamos ViewBag para pasar esta lista "extra" a la Vista.
            //    ViewBag es un "cajón" temporal para pasar datos 
            //    además del @model principal.
            ViewBag.Productos = productosDisponibles;

            // 3. Pasamos el presupuesto (el @model) a la Vista
            return View(presupuesto);
        }
        catch (Exception ex)
        {
            // Manejo de error genérico
            return View("Error", ex.Message); // (Asumiendo que tienes una vista de Error)
        }
    }

    // --- CREAR (CREATE) - PASO 1: MOSTRAR EL FORMULARIO (GET) ---
    // GET: /Presupuestos/Create
    [HttpGet]
    public IActionResult Create()
    {
        // Creamos un objeto Presupuesto nuevo y le asignamos
        // la fecha de hoy por defecto.
        var nuevoPresupuesto = new Presupuesto
        {
            FechaCreacion = DateTime.Now
        };

        // Pasamos el modelo a la vista para que pueda usar la fecha
        return View(nuevoPresupuesto);
    }

    // --- CREAR (CREATE) - PASO 2: RECIBIR EL FORMULARIO (POST) ---
    [HttpPost]
    public IActionResult Create(Presupuesto presupuesto)
    {
        // El 'presupuesto' que llega aquí solo tendrá 
        // NombreDestinatario y FechaCreacion (la lista de detalles estará vacía).
        presupuesto.FechaCreacion = DateTime.Now; // Aseguramos que la fecha sea la actual

        if (ModelState.IsValid)
        {
            try
            {
                // 1. Usamos el repositorio para crear el encabezado en la BD
                //    (Nuestro repo ya maneja transacciones, ¡está perfecto!)
                var presupuestoCreado = _presupuestoRepository.Create(presupuesto);

                // 2. ¡Redirigimos al usuario a la vista de Detalles
                //    para que pueda empezar a agregar productos!
                return RedirectToAction("Details", new { id = presupuestoCreado.IdPresupuesto });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(presupuesto);
            }
        }
        return View(presupuesto);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var presupuesto = _presupuestoRepository.GetById(id);
        if (presupuesto == null)
        {
            return NotFound();
        }
        // Mostramos el formulario de edición, RELLENO con los datos
        return View(presupuesto);
    }

    // --- EDITAR (UPDATE) - PASO 2: RECIBIR EL FORMULARIO (POST) ---
    // POST: /Presupuestos/Edit/5
    [HttpPost]
    public IActionResult Edit(Presupuesto presupuesto)
    {
        // Solo validamos y actualizamos el encabezado
        if (ModelState.IsValid)
        {
            try
            {
                // ¡Crearemos este método en el siguiente paso!
                _presupuestoRepository.Update(presupuesto);
                return RedirectToAction("Details", new { id = presupuesto.IdPresupuesto });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = ex.Message;
                return View(presupuesto);
            }
        }
        return View(presupuesto);
    }

    // --- ELIMINAR (DELETE) - PASO 1: MOSTRAR CONFIRMACIÓN (GET) ---
    // GET: /Presupuestos/Delete/5
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var presupuesto = _presupuestoRepository.GetById(id);
        if (presupuesto == null)
        {
            return NotFound();
        }
        // Mostramos la página de CONFIRMACIÓN de borrado
        return View(presupuesto);
    }

    // --- ELIMINAR (DELETE) - PASO 2: EJECUTAR EL BORRADO (POST) ---
    // POST: /Presupuestos/Delete/5
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            // Usamos el repo que ya tiene la lógica de transacción
            _presupuestoRepository.Delete(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Si hay un error, volvemos a mostrar la confirmación
            ViewData["ErrorMessage"] = ex.Message;
            var presupuesto = _presupuestoRepository.GetById(id);
            return View(presupuesto);
        }
    }

    [HttpPost]
    public IActionResult AgregarProducto(int presupuestoId, int productoId, int cantidad)
    {
        // Recibimos los 3 valores del formulario
        try
        {
            // Validamos que hayan seleccionado un producto
            if (productoId == 0)
            {
                throw new Exception("Debe seleccionar un producto.");
            }

            // 1. Usamos el método de nuestro repositorio que ya existía
            _presupuestoRepository.AgregarProductoAPresupuesto(presupuestoId, productoId, cantidad);

            // 2. Redirigimos de vuelta a la página de Detalles.
            //    Esto recargará la página y mostrará el producto
            //    recién agregado en la tabla.
            return RedirectToAction("Details", new { id = presupuestoId });
        }
        catch (Exception ex)
        {
            // Si hay un error (ej. producto duplicado, o no seleccionó), 
            // volvemos a Details pero mostrando el error.

            // Re-cargamos los datos necesarios para la vista Details
            var presupuesto = _presupuestoRepository.GetById(presupuestoId);
            ViewBag.Productos = _productoRepository.GetProducts();

            // Guardamos el mensaje de error para mostrarlo
            ViewData["ErrorMessage"] = ex.Message;

            return View("Details", presupuesto);
        }
    }
}
