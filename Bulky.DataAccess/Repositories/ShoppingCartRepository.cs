using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly ApplicationDbContext _context;

    public ShoppingCartRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(ShoppingCart shoppingCart)
    {
        _context.ShoppingCarts.Update(shoppingCart);
    }
}
