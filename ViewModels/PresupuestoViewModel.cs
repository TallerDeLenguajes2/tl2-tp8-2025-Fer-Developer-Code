using System.ComponentModel.DataAnnotations;
using System;
namespace SistemaVentas.Web.ViewModels
{
    public class PresupuestoViewModel
    {
        public int IdPresupuesto { get; set; }

        [Display(Name = "Nombre del Destinatario")]
     
        [Required(ErrorMessage = "El nombre del destinatario es obligatorio.")] // Requerido
        public string NombreDestinatario { get; set; }

        [Display(Name = "Fecha de Creación")]
       
        [Required(ErrorMessage = "La fecha es obligatoria.")] // Requerida 
        [DataType(DataType.Date)] // Ayuda al HTML a renderizar un control de fecha
        public DateTime FechaCreacion { get; set; }
    }
}