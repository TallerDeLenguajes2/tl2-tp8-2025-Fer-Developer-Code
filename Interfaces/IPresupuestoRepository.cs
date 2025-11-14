using Models;

namespace Interfaces;

public interface IPresupuestoRepository
    {
        Presupuesto Create(Presupuesto presupuesto);
        List<Presupuesto> GetAll();
        Presupuesto GetById(int id);
        void AgregarProductoAPresupuesto(int presupuestoId, int productoId, int cantidad);
        bool Update(Presupuesto presupuesto);
        bool Delete(int id);
        bool RemoverProductoDelPresupuesto(int presupuestoId, int productoId);
    }