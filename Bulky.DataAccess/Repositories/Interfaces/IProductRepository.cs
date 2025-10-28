using Bulky.Models;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    void Update(Product product);
}
