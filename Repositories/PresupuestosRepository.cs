using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TP7.PresupuestoModel;
using TP7.PresupuestosDetalleModel;
using TP7.ProductosModel;

namespace TP7.PresupuestosRepositorySpace;

/// <summary>
/// Repositorio para gestionar las operaciones de Presupuestos.
/// Esta clase maneja la lógica de transacciones, ya que un Presupuesto
/// se compone de un encabezado y una lista de detalles (dos tablas).
/// </summary>
public class PresupuestosRepository
{
    private readonly string _cadenaConexion;

    public PresupuestosRepository()
    {
        _cadenaConexion = "Data Source=DB/Tienda.db";
    }

    /// <summary>
    /// Crea un nuevo presupuesto (encabezado y detalles) usando una transacción.
    /// </summary>
    /// <remarks>
    /// --- ¿QUÉ ES UNA TRANSACCIÓN? ---
    /// Es una operación "todo o nada".
    /// Si falla la inserción del detalle 2 de 5, la transacción
    /// deshace (Rollback) la inserción del encabezado y del detalle 1.
    /// Esto previene que queden "datos huérfanos" en la base de datos.
    /// </remarks>
    public Presupuesto Create(Presupuesto presupuesto)
    {
        const string queryPresupuesto = @"
                INSERT INTO Presupuestos (NombreDestinatario, FechaCreacion) 
                VALUES (@nombre, @fecha);
                SELECT last_insert_rowid();";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();

                // 1. Iniciar la Transacción
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        // 2. Crear el Presupuesto (encabezado)
                        using (var command = new SqliteCommand(queryPresupuesto, conexion, transaction))
                        {
                            command.Parameters.AddWithValue("@nombre", presupuesto.NombreDestinatario);
                            command.Parameters.AddWithValue("@fecha", presupuesto.FechaCreacion);
                            presupuesto.IdPresupuesto = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // 3. Crear los Detalles (asociados a la misma transacción)
                        if (presupuesto.Detalle != null)
                        {
                            foreach (var detalle in presupuesto.Detalle)
                            {
                                // Usamos un método helper para no repetir código
                                AgregarProductoDetalle(
                                    presupuesto.IdPresupuesto,
                                    detalle.Producto.IdProducto,
                                    detalle.Cantidad,
                                    conexion,
                                    transaction
                                );
                            }
                        }

                        // 4. ¡ÉXITO! Confirmar todos los cambios.
                        transaction.Commit();
                        return presupuesto;
                    }
                    catch
                    {
                        // 5. ¡FALLO! Deshacer todos los cambios.
                        transaction.Rollback();
                        throw; // Re-lanzar la excepción para que el controlador la atrape
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al crear presupuesto: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Obtiene todos los presupuestos, incluyendo sus detalles.
    /// </summary>
    public List<Presupuesto> GetAll()
    {
        var presupuestos = new List<Presupuesto>();
        const string query = "SELECT IdPresupuesto, NombreDestinatario, FechaCreacion FROM Presupuestos";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var presupuesto = new Presupuesto
                            {
                                IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                                NombreDestinatario = reader["NombreDestinatario"].ToString(),
                                FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()),

                                // Por cada presupuesto, buscamos sus detalles
                                // (Esto puede ser lento si hay miles de presupuestos,
                                // se conoce como "N+1 query problem", pero es
                                // la forma más simple de empezar).
                                Detalle = ObtenerDetallesPresupuesto(Convert.ToInt32(reader["IdPresupuesto"]), conexion)
                            };
                            presupuestos.Add(presupuesto);
                        }
                    }
                }
            }
            return presupuestos;
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener todos los presupuestos: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Obtiene un presupuesto por ID, incluyendo sus detalles.
    /// </summary>
    public Presupuesto GetById(int id)
    {
        const string query = "SELECT IdPresupuesto, NombreDestinatario, FechaCreacion FROM Presupuestos WHERE IdPresupuesto = @id";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                using (var command = new SqliteCommand(query, conexion))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Presupuesto
                            {
                                IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                                NombreDestinatario = reader["NombreDestinatario"].ToString(),
                                FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()),
                                Detalle = ObtenerDetallesPresupuesto(id, conexion)
                            };
                        }
                    }
                }
            }
            return null; // No se encontró
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener presupuesto {id}: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Agrega un nuevo producto (detalle) a un presupuesto existente.
    /// </summary>
    public void AgregarProductoAPresupuesto(int presupuestoId, int productoId, int cantidad)
    {
        // Esta operación es "atómica" (una sola inserción),
        // por lo que no necesita una transacción explícita.
        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                AgregarProductoDetalle(presupuestoId, productoId, cantidad, conexion, null);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al agregar producto {productoId} al presupuesto {presupuestoId}: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Elimina un presupuesto y todos sus detalles asociados usando una transacción.
    /// </summary>
    public bool Delete(int id)
    {
        const string queryDetalle = "DELETE FROM PresupuestosDetalle WHERE IdPresupuesto = @id";
        const string queryPresupuesto = "DELETE FROM Presupuestos WHERE IdPresupuesto = @id";

        try
        {
            using (var conexion = new SqliteConnection(_cadenaConexion))
            {
                conexion.Open();
                // 1. Iniciar la Transacción
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        // 2. Eliminar los Detalles
                        using (var commandDetalle = new SqliteCommand(queryDetalle, conexion, transaction))
                        {
                            commandDetalle.Parameters.AddWithValue("@id", id);
                            commandDetalle.ExecuteNonQuery();
                        }

                        // 3. Eliminar el Encabezado
                        using (var commandPresupuesto = new SqliteCommand(queryPresupuesto, conexion, transaction))
                        {
                            commandPresupuesto.Parameters.AddWithValue("@id", id);
                            int rowsAffected = commandPresupuesto.ExecuteNonQuery();

                            // 4. ¡ÉXITO! Confirmar.
                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        // 5. ¡FALLO! Deshacer.
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al eliminar presupuesto {id}: " + ex.Message, ex);
        }
    }


    // --- MÉTODOS PRIVADOS (Helpers) ---

    /// <summary>
    /// Método helper interno para agregar un detalle de producto.
    /// Puede operar dentro de una transacción existente o por sí solo.
    /// </summary>
    private void AgregarProductoDetalle(
        int presupuestoId,
        int productoId,
        int cantidad,
        SqliteConnection conexion,
        SqliteTransaction transaction) // 'transaction' puede ser null
    {
        const string query = @"
                INSERT INTO PresupuestosDetalle (IdPresupuesto, IdProducto, Cantidad) 
                VALUES (@presupuestoId, @productoId, @cantidad)";

        // El comando se asocia a la conexión Y a la transacción
        using (var command = new SqliteCommand(query, conexion, transaction))
        {
            command.Parameters.AddWithValue("@presupuestoId", presupuestoId);
            command.Parameters.AddWithValue("@productoId", productoId);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Método helper para obtener la lista de detalles de un presupuesto.
    /// </summary>
    /// <remarks>
    /// --- ¿QUÉ ES UN INNER JOIN? ---
    /// Es un comando SQL que combina filas de dos tablas.
    /// Aquí, combina 'PresupuestosDetalle' (pd) con 'Productos' (p)
    /// donde sus 'IdProducto' coincidan.
    /// Esto nos permite obtener la Cantidad, Descripcion y Precio en una sola consulta.
    /// </remarks>
    private List<PresupuestosDetalle> ObtenerDetallesPresupuesto(int presupuestoId, SqliteConnection conexion)
    {
        const string query = @"
                SELECT pd.Cantidad, p.IdProducto, p.Descripcion, p.Precio
                FROM PresupuestosDetalle pd 
                INNER JOIN Productos p ON pd.IdProducto = p.IdProducto 
                WHERE pd.IdPresupuesto = @id";

        var detalles = new List<PresupuestosDetalle>();

        // No necesitamos 'try...catch' aquí porque el método público 
        // que lo llama ya tiene uno.

        using (var command = new SqliteCommand(query, conexion))
        {
            command.Parameters.AddWithValue("@id", presupuestoId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var detalle = new PresupuestosDetalle
                    {
                        Cantidad = Convert.ToInt32(reader["Cantidad"]),
                        Producto = new Productos
                        {
                            IdProducto = Convert.ToInt32(reader["IdProducto"]),
                            Descripcion = reader["Descripcion"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"])
                        }
                    };
                    detalles.Add(detalle);
                }
            }
        }
        return detalles;
    }
}
