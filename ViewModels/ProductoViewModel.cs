using System.ComponentModel.DataAnnotations;
namespace SistemaVentas.Web.ViewModels
{
    public class ProductoViewModel
    {
        // Guardamos el ID para saber qué producto editar
        public int IdProducto { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(250, ErrorMessage = "La descripción no puede tener más de 250 caracteres.")]
        public string Descripcion { get; set; } // Opcional, pero con límite 

        [Display(Name = "Precio Unitario")]
       
        [Required(ErrorMessage = "El precio es obligatorio.")] // Requerido 
     
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser un valor positivo.")] // Mayor a 0
        public decimal Precio { get; set; }
    }
}