using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
    }
}
