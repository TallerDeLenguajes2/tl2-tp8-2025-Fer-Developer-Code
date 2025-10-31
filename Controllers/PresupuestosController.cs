using Microsoft.AspNetCore.Mvc;
using TP7.PresupuestoModel;
using TP7.PresupuestosRepositorySpace;
using TP7.ProductoRepositorySpace;
using TP7.ProductosModel;

namespace TP8.Controllers;

public class PresupuestoController : Controller
{
    private PresupuestosRepository presupuestoRepository;
    public PresupuestoController()
    {
        presupuestoRepository = new PresupuestosRepository();
    }
    [HttpGet]
    public IActionResult Index()
    {
        List<Presupuesto> presupuestos = presupuestoRepository.GetAll();
        return View(presupuestos);
    }
}
