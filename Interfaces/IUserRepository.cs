using Models;
namespace Interfaces;

public interface IUserRepository
    {
        // Define el único método que pide el TP:
        // Retorna el objeto Usuario si las credenciales son válidas, sino null[cite: 2265].
        Usuario GetUser(string username, string password);
    }