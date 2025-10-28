using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICategoryRepository CategoryRepository { get; private set; }
    public IProductRepository ProductRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext context,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _context = context;
        CategoryRepository = categoryRepository;
        ProductRepository = productRepository;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
