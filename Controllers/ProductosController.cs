using Microsoft.AspNetCore.Mvc;
using TP7.ProductoRepositorySpace;
using TP7.ProductosModel;

namespace TP8.Controllers;

public class ProductosController : Controller
{
    private ProductoRepository productoRepository;
    public ProductosController()
    {
        productoRepository = new ProductoRepository();
    }
    [HttpGet]
    public IActionResult Index()
    {
        List<Productos> productos = productoRepository.GetProducts();
        return View(productos);
    }
}
