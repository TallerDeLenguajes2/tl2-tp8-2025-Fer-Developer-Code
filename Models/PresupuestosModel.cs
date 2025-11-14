using System;
using System.Collections.Generic;

namespace Models;

public class Presupuesto
{
    public int IdPresupuesto { get; set; }
    public string NombreDestinatario { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<PresupuestosDetalle> Detalle { get; set; } = new List<PresupuestosDetalle>();

    public decimal MontoPresupuesto()
    {
        decimal montoTotal = 0;
        foreach(var item in Detalle)
        {
            montoTotal += item.Producto.Precio * item.Cantidad;
        }
        return montoTotal;
    }

    public decimal MontoPresupuestoConIVA()
    {
        decimal montoBase = MontoPresupuesto();
        decimal montoConIva = montoBase * 1.21m;
        return montoConIva;
    }

    public int CantidadProductos()
    {
        int cantidadTotal = 0;
        foreach(var item in Detalle)
        {
            cantidadTotal += item.Cantidad;
        }
        return cantidadTotal;
    }
}