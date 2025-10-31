using System;
using System.Collections.Generic;
using TP7.PresupuestosDetalleModel;

namespace TP7.PresupuestoModel;

public class Presupuesto
{
    public int IdPresupuesto { get; set; }
    public string NombreDestinatario { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<PresupuestosDetalle> Detalle { get; set; }

    public double MontoPresupuesto()
    {
        double montoTotal = 0;
        foreach(var item in Detalle)
        {
            montoTotal += item.Producto.Precio * item.Cantidad;
        }
        return montoTotal;
    }

    public double MontoPresupuestoConIVA()
    {
        double montoBase = MontoPresupuesto();
        double montoConIva = montoBase * 1.21;
        return (double)montoConIva;
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