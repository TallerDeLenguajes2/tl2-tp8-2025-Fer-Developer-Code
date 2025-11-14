using Microsoft.Data.Sqlite;
using Interfaces;
using Models;

namespace Repositories;

public class UsuarioRepository : IUserRepository
{
    private readonly string _cadenaConexion;

    public UsuarioRepository()
    {
        _cadenaConexion = "Data Source=DB/Tienda.db";
    }

    /// <summary>
    /// Busca un usuario en la BD por nombre de usuario y contraseña.
    /// </summary>
    /// <returns>El objeto Usuario si lo encuentra, o null si no existe.</returns>
    public Usuario GetUser(string username, string password)
    {
        // Consulta SQL exacta para buscar por usuario y contraseña
        const string query = @"
                SELECT Id, Nombre, User, Pass, Rol
                FROM Usuarios
                WHERE User = @Username AND Pass = @Password";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var comando = new SqliteCommand(query, conexion))
                {
                    // Usamos parámetros para prevenir Inyección SQL 
                    comando.Parameters.AddWithValue("@Username", username);
                    comando.Parameters.AddWithValue("@Password", password);

                    using (var reader = comando.ExecuteReader())
                    {
                        // Si el lector encuentra una fila, el usuario existe
                        if (reader.Read())
                        {
                            // Leemos el string "Administrador" o "Cliente" de la BD
                            string rolComoString = reader["Rol"].ToString();

                            return new Usuario
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Nombre = reader["Nombre"].ToString(),
                                User = reader["User"].ToString(),
                                Pass = reader["Pass"].ToString(),

                                Rol = Enum.Parse<Roles>(rolComoString)
                            };
                        }
                    }
                }
            }

            // Si el 'if (reader.Read())' fue falso, no encontró a nadie
            return null;
        }
        catch (Exception ex)
        {
            // Es buena práctica lanzar un error más específico
            throw new Exception("Error al obtener el usuario desde la base de datos: " + ex.Message, ex);
        }
    }
}