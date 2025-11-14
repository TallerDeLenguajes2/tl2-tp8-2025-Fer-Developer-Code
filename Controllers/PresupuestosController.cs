using Microsoft.AspNetCore.Mvc;
using Models;
using Repositories;
using SistemaVentas.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Interfaces;

namespace TP8.Controllers;

public class PresupuestosController : Controller
{
    // 2. CAMBIA LOS TIPOS A LAS INTERFACES (y readonly)
    private readonly IPresupuestoRepository _presupuestoRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IAuthenticationService _authService;

    // ASP.NET verá que pides DOS dependencias y te las entregará
    public PresupuestosController(IPresupuestoRepository presupuestoRepository,
                                    IProductoRepository productoRepository,
                                    IAuthenticationService authService)
    {
        _presupuestoRepository = presupuestoRepository;
        _productoRepository = productoRepository;
        _authService = authService;
    }

    private IActionResult CheckAdminPermissions()
    {
        if (!_authService.IsAuthenticated()) return RedirectToAction("Index", "Login");
        if (!_authService.HasAccessLevel("Administrador"))
        {
            // Lo mandamos a su propia vista de Acceso Denegado
            return RedirectToAction(nameof(AccesoDenegado));
        }
        return null;
    }

    private IActionResult CheckClientOrAdminPermissions()
    {
        if (!_authService.IsAuthenticated()) return RedirectToAction("Index", "Login");

        // Revisa si NO es Admin Y TAMPOCO es Cliente
        if (!_authService.HasAccessLevel("Administrador") &&
            !_authService.HasAccessLevel("Cliente"))
        {
            return RedirectToAction("accesoDenegado", "Home");
        }
        return null;
    }

    // 3. Acción Index: Responde a GET /Presupuestos
    public IActionResult Index()
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        // 4. Pedimos la lista COMPLETA de presupuestos al repo
        //    (El repo se encarga de traer los detalles de c/u)
        List<Presupuesto> presupuestos = _presupuestoRepository.GetAll();

        // 5. Pasamos la lista a la vista "Index.cshtml"
        return View(presupuestos);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
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
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        // Creamos un ViewModel nuevo y asignamos la fecha por defecto
        var viewModel = new PresupuestoViewModel
        {
            FechaCreacion = DateTime.Now
        };
        return View(viewModel);
    }

    // --- CREAR (CREATE) - PASO 2: RECIBIR EL FORMULARIO (POST) ---
    [HttpPost]
    public IActionResult Create(PresupuestoViewModel viewModel) // 1. RECIBE EL VIEWMODEL
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        try
        {
            // 2. ¡VALIDACIÓN PERSONALIZADA!
            //    Requisito del TP9: La fecha no puede ser futura
            if (viewModel.FechaCreacion.Date > DateTime.Now.Date)
            {
                // Añadimos un error personalizado al ModelState
                ModelState.AddModelError(nameof(viewModel.FechaCreacion), "La fecha no puede ser futura.");
            }

            // 3. CHEQUEO DE VALIDACIÓN (Data Annotations + el nuestro)
            if (!ModelState.IsValid)
            {
                // Si falla, volvemos a mostrar el formulario
                return View(viewModel);
            }

            // 4. "MAPEO": ViewModel -> Modelo de BD
            var presupuestoBD = new Presupuesto
            {
                NombreDestinatario = viewModel.NombreDestinatario,
                FechaCreacion = viewModel.FechaCreacion
                // La lista de detalles va vacía, como antes
            };

            // 5. Enviamos al repositorio
            var presupuestoCreado = _presupuestoRepository.Create(presupuestoBD);

            return RedirectToAction("Details", new { id = presupuestoCreado.IdPresupuesto });
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(viewModel);
        }
    }
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        var presupuestoBD = _presupuestoRepository.GetById(id);
        if (presupuestoBD == null)
        {
            return NotFound();
        }

        // "MAPEO INVERSO": Modelo de BD -> ViewModel
        var viewModel = new PresupuestoViewModel
        {
            IdPresupuesto = presupuestoBD.IdPresupuesto,
            NombreDestinatario = presupuestoBD.NombreDestinatario,
            FechaCreacion = presupuestoBD.FechaCreacion
        };

        return View(viewModel);
    }

    // --- EDITAR (UPDATE) - PASO 2: RECIBIR EL FORMULARIO (POST) ---
    // POST: /Presupuestos/Edit/5
    [HttpPost]
    public IActionResult Edit(PresupuestoViewModel viewModel) // 👈 1. RECIBE EL VIEWMODEL
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        try
        {
            // 2. ¡VALIDACIÓN PERSONALIZADA!
            if (viewModel.FechaCreacion.Date > DateTime.Now.Date)
            {
                ModelState.AddModelError(nameof(viewModel.FechaCreacion), "La fecha no puede ser futura.");
            }

            // 3. CHEQUEO DE VALIDACIÓN
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            // 4. "MAPEO": ViewModel -> Modelo de BD
            var presupuestoBD = new Presupuesto
            {
                IdPresupuesto = viewModel.IdPresupuesto,
                NombreDestinatario = viewModel.NombreDestinatario,
                FechaCreacion = viewModel.FechaCreacion
            };

            // 5. Enviamos al repositorio
            _presupuestoRepository.Update(presupuestoBD);

            return RedirectToAction("Details", new { id = presupuestoBD.IdPresupuesto });
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(viewModel);
        }
    }
    // --- ELIMINAR (DELETE) - PASO 1: MOSTRAR CONFIRMACIÓN (GET) ---
    // GET: /Presupuestos/Delete/5
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
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

    [HttpGet]
    public IActionResult AgregarProducto(int id) // 'id' es el IdPresupuesto
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        // 1. Obtenemos la lista de productos para el dropdown
        var productosDisponibles = _productoRepository.GetProducts();

        // 2. Creamos el ViewModel
        var viewModel = new AgregarProductoViewModel
        {
            IdPresupuesto = id, // Pasamos el ID del presupuesto al que pertenece

            // 3. Creamos el SelectList para el dropdown 
            //    - "productosDisponibles" es la lista de datos.
            //    - "IdProducto" es el nombre de la propiedad que se usará como valor (el ID).
            //    - "Descripcion" es el nombre de la propiedad que verá el usuario.
            ListaProductos = new SelectList(productosDisponibles, "IdProducto", "Descripcion")
        };

        // 4. Pasamos el ViewModel a la nueva vista
        return View(viewModel);
    }

    // --- ACCIÓN "AGREGAR PRODUCTO" (POST) ---
    // POST: /Presupuestos/AgregarProducto
    [HttpPost]
    public IActionResult AgregarProducto(AgregarProductoViewModel viewModel)
    {
        var securityCheck = CheckClientOrAdminPermissions();
        if (securityCheck != null) return securityCheck;
        try
        {
            // 1. CHEQUEO DE VALIDACIÓN
            if (!ModelState.IsValid)
            {
                // ¡LÓGICA CRÍTICA DE RECARGA!
                // Si la validación falla (ej. cantidad en 0),
                // el `viewModel.ListaProductos` es 'null' (porque no viaja en el POST).
                // Debemos recargarlo antes de devolver la vista .
                var productos = _productoRepository.GetProducts();
                viewModel.ListaProductos = new SelectList(productos, "IdProducto", "Descripcion");

                return View(viewModel); // Devolvemos la vista con el error y el dropdown lleno
            }

            // 2. Si es válido, llamamos al repositorio
            _presupuestoRepository.AgregarProductoAPresupuesto(
                viewModel.IdPresupuesto,
                viewModel.IdProducto,
                viewModel.Cantidad
            );

            // 3. Redirigimos de vuelta al Detalle del presupuesto
            return RedirectToAction("Details", new { id = viewModel.IdPresupuesto });
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            // Recargamos el SelectList también en caso de un error de BD
            var productos = _productoRepository.GetProducts();
            viewModel.ListaProductos = new SelectList(productos, "IdProducto", "Descripcion");
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
