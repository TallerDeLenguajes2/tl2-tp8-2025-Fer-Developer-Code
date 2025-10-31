using Microsoft.Data.Sqlite;
using TP7.PresupuestoModel;
using TP7.PresupuestosDetalleModel;
using TP7.ProductosModel;
using System;
using System.Collections.Generic;

namespace TP7.PresupuestosRepositorySpace;

public class PresupuestosRepository
{
    private string cadenaConexion = "Data Source=DB/Tienda.db";

    public Presupuesto Create(Presupuesto presupuesto)
    {
        // CORREGIDO: Nombres de columnas
        var query = @"INSERT INTO Presupuestos (NombreDestinatario, FechaCreacion) 
                     VALUES (@nombre, @fecha);
                     SELECT last_insert_rowid();";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        
        command.Parameters.Add(new SqliteParameter("@nombre", presupuesto.NombreDestinatario));
        command.Parameters.Add(new SqliteParameter("@fecha", presupuesto.FechaCreacion));
        
        presupuesto.IdPresupuesto = Convert.ToInt32(command.ExecuteScalar());

        if (presupuesto.Detalle != null)
        {
            foreach (var detalle in presupuesto.Detalle)
            {
                // CORREGIDO: Nombres de columnas
                AgregarProductoAPresupuesto(presupuesto.IdPresupuesto, detalle.Producto.IdProducto, detalle.Cantidad);
            }
        }
        
        return presupuesto;
    }

    public List<Presupuesto> GetAll()
    {
        var presupuestos = new List<Presupuesto>();
        // CORREGIDO: Nombres de columnas
        var query = "SELECT IdPresupuesto, NombreDestinatario, FechaCreacion FROM Presupuestos";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var presupuesto = new Presupuesto
                {
                    // CORREGIDO: Nombres de columnas
                    IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                    NombreDestinatario = reader["NombreDestinatario"].ToString(),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()),
                    Detalle = ObtenerDetallesPresupuesto(Convert.ToInt32(reader["IdPresupuesto"]))
                };
                presupuestos.Add(presupuesto);
            }
        }
        return presupuestos;
    }

    public Presupuesto GetById(int id)
    {
        // CORREGIDO: Nombres de columnas
        var query = "SELECT IdPresupuesto, NombreDestinatario, FechaCreacion FROM Presupuestos WHERE IdPresupuesto = @id";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        command.Parameters.Add(new SqliteParameter("@id", id));
        
        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                return new Presupuesto
                {
                    // CORREGIDO: Nombres de columnas
                    IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                    NombreDestinatario = reader["NombreDestinatario"].ToString(),
                    FechaCreacion = DateTime.Parse(reader["FechaCreacion"].ToString()),
                    Detalle = ObtenerDetallesPresupuesto(id)
                };
            }
        }
        return null;
    }

    public void AgregarProductoAPresupuesto(int presupuestoId, int productoId, int cantidad)
    {
        // CORREGIDO: Nombres de columnas
        var query = @"INSERT INTO PresupuestosDetalle (IdPresupuesto, IdProducto, Cantidad) 
                     VALUES (@presupuestoId, @productoId, @cantidad)";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        
        command.Parameters.Add(new SqliteParameter("@presupuestoId", presupuestoId));
        command.Parameters.Add(new SqliteParameter("@productoId", productoId));
        command.Parameters.Add(new SqliteParameter("@cantidad", cantidad));
        
        command.ExecuteNonQuery();
    }

    public bool Delete(int id)
    {
        // CORREGIDO: Nombres de columnas
        var queryDetalle = "DELETE FROM PresupuestosDetalle WHERE IdPresupuesto = @id";
        var queryPresupuesto = "DELETE FROM Presupuestos WHERE IdPresupuesto = @id";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        
        var command = new SqliteCommand(queryDetalle, conexion);
        command.Parameters.Add(new SqliteParameter("@id", id));
        command.ExecuteNonQuery();
        
        command.CommandText = queryPresupuesto;
        var rowsAffected = command.ExecuteNonQuery();
        
        return rowsAffected > 0;
    }

    private List<PresupuestosDetalle> ObtenerDetallesPresupuesto(int presupuestoId)
    {
        var detalles = new List<PresupuestosDetalle>();
        // CORREGIDO: Nombres de columnas
        var query = @"SELECT pd.Cantidad, p.IdProducto, p.Descripcion, p.Precio 
                     FROM PresupuestosDetalle pd 
                     INNER JOIN Productos p ON pd.IdProducto = p.IdProducto 
                     WHERE pd.IdPresupuesto = @id";
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        command.Parameters.Add(new SqliteParameter("@id", presupuestoId));
        
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
        return detalles;
    }
}