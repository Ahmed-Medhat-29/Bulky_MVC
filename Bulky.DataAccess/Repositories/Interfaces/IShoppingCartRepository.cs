using Bulky.Models;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    void Update(ShoppingCart shoppingCart);
}
