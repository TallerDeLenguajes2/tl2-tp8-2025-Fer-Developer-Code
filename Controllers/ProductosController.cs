using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Web.ViewModels;
using Models;
using Repositories;
using Interfaces;

namespace TP8.Controllers;

public class ProductosController : Controller
{
    // 2. CAMBIA EL TIPO A LA INTERFAZ 
    private readonly IProductoRepository _productoRepository;
    private readonly IAuthenticationService _authService;

    // 3. REEMPLAZA TU CONSTRUCTOR VACÍO POR ESTE
    // Este es el "Constructor de Inyección de Dependencias"
    // Pide la "abstracción" (la interfaz), no la clase concreta
    public ProductosController(IProductoRepository productoRepository, IAuthenticationService authService)
    {
        // 4. Asigna la dependencia que ASP.NET te "inyecta"
        _productoRepository = productoRepository;
        _authService = authService;
    }

    // 5. CREA EL MÉTODO DE CHEQUEO PRIVADO
    private IActionResult CheckAdminPermissions()
    {
        // 1. ¿No está logueado? -> A la página de Login
        if (!_authService.IsAuthenticated())
        {
            return RedirectToAction("Index", "Login");
        }

        // 2. ¿No es Admin? -> A la página de Acceso Denegado
        if (!_authService.HasAccessLevel("Administrador"))
        {
            return RedirectToAction("AccesoDenegado", "Home"); // Asumiendo que está en Shared
        }

        // 3. Si pasó ambos filtros, tiene permiso
        return null;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;

        List<Productos> productos = _productoRepository.GetProducts();
        return View(productos);
    }
    /// <summary>
    /// Devuelve la vista Create.cshtml (un formulario para crear un nuevo producto)
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Create()
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;
        return View(new ProductoViewModel());
    }
    /// <summary>
    /// Recibe el producto desde el formulario Create.cshtml y lo guarda en la base de datos
    /// </summary>
    /// <param name="productoVM"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Create(ProductoViewModel productoVM)
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;

        try
        {
            if (!ModelState.IsValid)
            {

                return View(productoVM);
            }

            var productoDB = new Productos
            {
                Descripcion = productoVM.Descripcion,
                Precio = productoVM.Precio,
            };
            _productoRepository.Create(productoDB);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(productoVM);
        }
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;
        Productos producto = _productoRepository.GetById(id);
        if (producto == null)
        {
            return NotFound();
        }
        return View(producto);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;
        var ProductoDB = _productoRepository.GetById(id);
        if (ProductoDB == null)
        {
            return NotFound();
        }

        var productoVM = new ProductoViewModel
        {
            IdProducto = ProductoDB.IdProducto,
            Descripcion = ProductoDB.Descripcion,
            Precio = ProductoDB.Precio
        };
        return View(productoVM);
    }

    [HttpPost]
    public IActionResult Edit(ProductoViewModel productoVM)
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;
        try
        {
            // 3. ¡CHEQUEO DE VALIDACIÓN!
            if (!ModelState.IsValid)
            {
                return View(productoVM);
            }

            // 4. "MAPEO": Convertimos el ViewModel al Modelo de BD
            var productoBD = new Productos
            {
                IdProducto = productoVM.IdProducto,
                Descripcion = productoVM.Descripcion,
                Precio = productoVM.Precio
            };

            // 5. Enviamos el modelo de BD al repositorio
            _productoRepository.Update(productoBD.IdProducto, productoBD);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(productoVM);
        }
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var securityCheck = CheckAdminPermissions();
        if (securityCheck != null) return securityCheck;
        var producto = _productoRepository.GetById(id);
        if (producto == null)
        {
            return NotFound();
        }
        // Muestra la página de CONFIRMACIÓN de borrado
        return View(producto);
    }

    // --- ELIMINAR (DELETE) - PASO 2 (POST) ---
    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")] // [ActionName] evita conflictos de nombres
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _productoRepository.Delete(id);
            return RedirectToAction("Index"); // Vuelve al listado
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            // Si hay un error (ej. clave foránea), volvemos a mostrar la confirmación
            var producto = _productoRepository.GetById(id);
            return View(producto);
        }
    }
}
