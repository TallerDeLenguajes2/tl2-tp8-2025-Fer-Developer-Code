using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectList 
namespace SistemaVentas.Web.ViewModels
{
    public class AgregarProductoViewModel
    {
        // ID del presupuesto al que agregamos productos
        // (lo pasaremos en un campo oculto)
        public int IdPresupuesto { get; set; }

        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")] // Hacemos que el dropdown sea obligatorio
        public int IdProducto { get; set; } // Guardará el ID del producto seleccionado

        [Display(Name = "Cantidad")]
        [Required(ErrorMessage = "La cantidad es obligatoria.")] // Requerida [cite: 1426]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")] // Mayor a 0 [cite: 1426]
        public int Cantidad { get; set; } = 1; // Valor por defecto

        // Propiedad para rellenar el DropDown
        // No se envía desde el formulario, la rellena el Controlador
        public SelectList? ListaProductos { get; set; }
    }
}