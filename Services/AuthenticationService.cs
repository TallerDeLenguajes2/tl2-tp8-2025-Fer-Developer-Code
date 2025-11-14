using Interfaces; // Para las interfaces
using Models;   // Para el modelo Usuario
using Microsoft.AspNetCore.Http; // ¡Fundamental para la Sesión!

namespace Services;

public class AuthenticationService : IAuthenticationService
{
    // --- INYECCIÓN DE DEPENDENCIAS ---
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    // --- IMPLEMENTACIÓN DE MÉTODOS ---

    public bool Login(string username, string password)
    {
        // 1. Usamos el repositorio (inyectado) para buscar al usuario
        var user = _userRepository.GetUser(username, password);

        // 2. Si no existe, el login falla
        if (user == null)
        {
            return false;
        }

        // 3. ¡Login exitoso! Guardamos los datos en la Sesión
        var context = _httpContextAccessor.HttpContext;
        context.Session.SetString("IsAuthenticated", "true");
        context.Session.SetString("Username", user.User);
        context.Session.SetString("UserNombre", user.Nombre);

        // Usamos la lógica del enum (¡mucho más segura!)
        context.Session.SetString("Rol", user.Rol.ToString());

        return true;
    }

    public void Logout()
    {
        // Limpiamos toda la sesión
        _httpContextAccessor.HttpContext.Session.Clear();
    }

    public bool IsAuthenticated()
    {
        // Leemos la sesión para ver si está logueado
        return _httpContextAccessor.HttpContext.Session.GetString("IsAuthenticated") == "true";
    }

    public bool HasAccessLevel(string requiredAccessLevel)
    {
        // Leemos el Rol guardado en la sesión
        var userRol = _httpContextAccessor.HttpContext.Session.GetString("Rol");

        // Comparamos si el rol del usuario es el que se requiere
        return userRol == requiredAccessLevel;
    }
}