using Models;

namespace Interfaces;

public interface IProductoRepository
    {
        List<Productos> GetProducts();
        Productos GetById(int id);
        Productos Create(Productos producto);
        bool Update(int id, Productos producto);
        bool Delete(int id);
    }