using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using TP7.ProductosModel;
using System;

namespace TP7.ProductoRepositorySpace;

public class ProductoRepository
{
    // El 'DB/Tienda.db' debe estar en la raíz de tu proyecto API
    private string cadenaConexion = "Data Source=DB/Tienda.db";

    public List<Productos> GetProducts()
    {
        // CORREGIDO: Consulta usa 'IdProducto', 'Descripcion', 'Precio'
        string query = "SELECT IdProducto, Descripcion, Precio FROM Productos";
        List<Productos> listadoProductos = new List<Productos>();
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var product = new Productos(){
                    // CORREGIDO: Nombres de columnas con mayúsculas
                    IdProducto = Convert.ToInt32(reader["IdProducto"]),
                    Descripcion = reader["Descripcion"].ToString(),
                    Precio = Convert.ToDecimal(reader["Precio"]) 
                };
                listadoProductos.Add(product);
            }
        }
        conexion.Close();
        return listadoProductos;
    }

    public Productos Create(Productos producto)
    {
        // CORREGIDO: Nombres de columnas
        var query = @"INSERT INTO Productos (Descripcion, Precio) VALUES (@descripcion, @precio);
                     SELECT last_insert_rowid();"; 
        
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        
        command.Parameters.Add(new SqliteParameter("@descripcion", producto.Descripcion));
        command.Parameters.Add(new SqliteParameter("@precio", producto.Precio));
        
        // ExecuteScalar devuelve un 'long' (Int64), es más seguro convertirlo así
        producto.IdProducto = Convert.ToInt32(command.ExecuteScalar());
        conexion.Close();
        
        return producto;
    }

    public bool Update(int id, Productos producto)
    {
        // CORREGIDO: Nombres de columnas
        var query = "UPDATE Productos SET Descripcion = @descripcion, Precio = @precio WHERE IdProducto = @id";
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        
        command.Parameters.Add(new SqliteParameter("@descripcion", producto.Descripcion));
        command.Parameters.Add(new SqliteParameter("@precio", producto.Precio));
        command.Parameters.Add(new SqliteParameter("@id", id));
        
        int rowsAffected = command.ExecuteNonQuery();
        conexion.Close();
        
        return rowsAffected > 0;
    }

    public Productos GetById(int id)
    {
        // CORREGIDO: Nombres de columnas
        var query = "SELECT IdProducto, Descripcion, Precio FROM Productos WHERE IdProducto = @id";
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        command.Parameters.Add(new SqliteParameter("@id", id));

        using (SqliteDataReader reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                var producto = new Productos()
                {
                    // CORREGIDO: Nombres de columnas
                    IdProducto = Convert.ToInt32(reader["IdProducto"]),
                    Descripcion = reader["Descripcion"].ToString(),
                    Precio = Convert.ToDecimal(reader["Precio"]) 
                };
                return producto;
            }
        }
        conexion.Close();
        return null;
    }

    public bool Delete(int id)
    {
        // CORREGIDO: Nombres de columnas
        var query = "DELETE FROM Productos WHERE IdProducto = @id";
        using var conexion = new SqliteConnection(cadenaConexion);
        conexion.Open();
        var command = new SqliteCommand(query, conexion);
        command.Parameters.Add(new SqliteParameter("@id", id));
        
        int rowsAffected = command.ExecuteNonQuery();
        conexion.Close();
        
        return rowsAffected > 0;
    }
}