using System.ComponentModel.DataAnnotations;
namespace ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    // Esta propiedad la usaremos para mostrar mensajes
    // como "Credenciales inválidas"
    public string ErrorMessage { get; set; }
}