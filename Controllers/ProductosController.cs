using Microsoft.AspNetCore.Mvc;
using TP7.ProductoRepositorySpace;
using TP7.ProductosModel;

namespace TP8.Controllers;

public class ProductosController : Controller
{
    private ProductoRepository _productoRepository;
    public ProductosController()
    {
        _productoRepository = new ProductoRepository();
    }
    [HttpGet]
    public IActionResult Index()
    {
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
        return View();
    }
    /// <summary>
    /// Recibe el producto desde el formulario Create.cshtml y lo guarda en la base de datos
    /// </summary>
    /// <param name="producto"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Create(Productos producto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _productoRepository.Create(producto);
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(producto);
        }
        // Si el ModelState no es válido, volvemos a mostrar el formulario
        return View(producto);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
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
        Productos producto = _productoRepository.GetById(id);
        if (producto == null)
        {
            return NotFound();
        }
        return View(producto);
    }

    [HttpPost]
    public IActionResult Edit(Productos producto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                _productoRepository.Update(producto.IdProducto, producto);
                return RedirectToAction("Index");
            }

            return View(producto);
        }
        catch (Exception ex)
        {
            ViewData["ErrorMessage"] = ex.Message;
            return View(producto);
        }
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
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
