using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TP7.ProductosModel;

namespace TP7.ProductoRepositorySpace;

/// <summary>
/// Repositorio para gestionar las operaciones de Productos en la base de datos.
/// Sigue el Principio de Responsabilidad Única (SRP):
/// Su única responsabilidad es mediar entre la lógica de negocio y la base de datos.
/// </summary>
public class ProductoRepository
{
    // 'readonly' asegura que la cadena de conexión no se pueda cambiar
    // después de que se inicializa el repositorio.
    private readonly string _cadenaConexion;

    // --- Los 3 OBJETOS CLAVE de ADO.NET ---
    // 1. SqliteConnection: Representa la conexión física a la base de datos.
    // 2. SqliteCommand: Representa la consulta SQL que queremos ejecutar.
    // 3. SqliteDataReader: Es un cursor para leer los resultados de un SELECT, 
    //    fila por fila, de forma muy eficiente.
    // ----------------------------------------

    public ProductoRepository()
    {
        // "Data Source" es la clave que usa SQLite para saber dónde está el archivo .db
        _cadenaConexion = "Data Source=DB/Tienda.db";
    }

    /// <summary>
    /// Obtiene una lista de todos los productos de la base de datos.
    /// </summary>
    /// <returns>Una List del tipo Productos</returns>
    public List<Productos> GetProducts()
    {
        // 'const' para strings que no cambian, como las consultas SQL.
        // Usar '@' permite strings multilínea para mayor legibilidad.
        const string query = @"
                SELECT IdProducto, 
                       Descripcion, 
                       Precio 
                FROM Productos";

        var listadoProductos = new List<Productos>();

        try
        {
            // 'using' es fundamental: asegura que la conexión se abra
            // y se CIERRE automáticamente, incluso si ocurre un error.
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();

                using (var command = new SqliteCommand(query, conexion))
                {
                    // --- Método de Ejecución 1: ExecuteReader() ---
                    // Se usa CUALQUIER VEZ que tu consulta sea un SELECT
                    // que pueda devolver una o más filas.
                    using (var reader = command.ExecuteReader())
                    {
                        // reader.Read() avanza al siguiente registro.
                        // Devuelve 'true' si hay un registro, 'false' si se terminaron.
                        while (reader.Read())
                        {
                            // "Hidratamos" el objeto:
                            // Convertimos los datos de la fila (reader) a un objeto C#.
                            var producto = new Productos
                            {
                                IdProducto = Convert.ToInt32(reader["IdProducto"]),
                                Descripcion = reader["Descripcion"].ToString(),
                                Precio = Convert.ToDecimal(reader["Precio"])
                            };
                            listadoProductos.Add(producto);
                        }
                    }
                }
            }
            return listadoProductos;
        }
        catch (Exception ex)
        {
            // Es buena práctica capturar la excepción original y lanzar
            // una nueva con un mensaje más específico de la operación.
            throw new Exception("Error al obtener productos: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Obtiene un único producto por su ID.
    /// </summary>
    /// <param name="id">El ID del producto a buscar.</param>
    /// <returns>El objeto Producto si se encuentra; null si no.</returns>
    public Productos GetById(int id)
    {
        const string query = @"
                SELECT IdProducto, 
                       Descripcion, 
                       Precio 
                FROM Productos 
                WHERE IdProducto = @id"; // @id es un PARÁMETRO

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    // --- SEGURIDAD: Prevención de SQL Injection ---
                    // NUNCA uses string.Format o '+' para armar consultas con datos del usuario.
                    // Usar .Parameters.AddWithValue() es la forma segura.
                    // ADO.NET se encarga de "limpiar" el valor 'id' antes de enviarlo a la BD.
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        // Usamos 'if' en lugar de 'while' porque esperamos
                        // como máximo un solo resultado (o ninguno).
                        if (reader.Read())
                        {
                            return new Productos
                            {
                                IdProducto = Convert.ToInt32(reader["IdProducto"]),
                                Descripcion = reader["Descripcion"].ToString(),
                                Precio = Convert.ToDecimal(reader["Precio"])
                            };
                        }
                    }
                }
            }
            // Si el 'if' no se cumple, significa que no se encontró
            // ningún producto con ese ID.
            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener producto {id}: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Crea un nuevo producto en la base de datos.
    /// </summary>
    /// <param name="producto">El objeto Producto a insertar (sin ID).</param>
    /// <returns>El mismo objeto Producto, actualizado con el nuevo ID generado por la BD.</returns>
    public Productos Create(Productos producto)
    {
        const string query = @"
                INSERT INTO Productos (
                    Descripcion, 
                    Precio
                ) VALUES (
                    @descripcion, 
                    @precio
                );
                SELECT last_insert_rowid();"; // Comando especial de SQLite para obtener el último ID insertado.

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@descripcion", producto.Descripcion);
                    command.Parameters.AddWithValue("@precio", producto.Precio);

                    // --- Método de Ejecución 2: ExecuteScalar() ---
                    // Se usa cuando tu consulta devuelve UN ÚNICO VALOR.
                    // (Ni filas completas, ni nada).
                    // Perfecto para `SELECT COUNT(*)` o `SELECT last_insert_rowid()`.
                    producto.IdProducto = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return producto;
        }
        catch (Exception ex)
        {
            throw new Exception("Error al crear producto: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Actualiza un producto existente en la base de datos.
    /// </summary>
    /// <param name="id">ID del producto a actualizar.</param>
    /// <param name="producto">Objeto con los nuevos datos.</param>
    /// <returns>true si la actualización fue exitosa, false si no.</returns>
    public bool Update(int id, Productos producto)
    {
        const string query = @"
                UPDATE Productos 
                SET Descripcion = @descripcion, 
                    Precio = @precio 
                WHERE IdProducto = @id";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@descripcion", producto.Descripcion);
                    command.Parameters.AddWithValue("@precio", producto.Precio);
                    command.Parameters.AddWithValue("@id", id);

                    // --- Método de Ejecución 3: ExecuteNonQuery() ---
                    // Se usa para todas las operaciones que NO devuelven datos:
                    // INSERT, UPDATE, DELETE.
                    // Devuelve el número de filas que fueron afectadas.
                    int rowsAffected = command.ExecuteNonQuery();

                    // Si rowsAffected > 0, significa que encontró el ID y lo actualizó.
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al actualizar producto {id}: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Elimina un producto de la base de datos por su ID.
    /// </summary>
    /// <param name="id">ID del producto a eliminar.</param>
    /// <returns>true si la eliminación fue exitosa, false si no.</returns>
    public bool Delete(int id)
    {
        const string query = "DELETE FROM Productos WHERE IdProducto = @id";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al eliminar producto {id}: " + ex.Message, ex);
        }
    }
}