namespace Interfaces;

public interface IAuthenticationService
{
    // Intenta loguear al usuario. Devuelve true si fue exitoso.
    bool Login(string username, string password);

    // Cierra la sesión del usuario.
    void Logout();

    // Verifica si el usuario actual está logueado.
    bool IsAuthenticated();

    // Verifica si el usuario actual tiene un rol específico (ej. "Administrador")
    bool HasAccessLevel(string requiredAccessLevel);
}