using Microsoft.AspNetCore.Mvc;
using SistemaVentas.Web.ViewModels;
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
