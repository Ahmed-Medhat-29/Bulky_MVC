using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICategoryRepository CategoryRepository { get; private set; }
    public IProductRepository ProductRepository { get; private set; }
    public ICompanyRepository CompanyRepository { get; private set; }

    public UnitOfWork(ApplicationDbContext context,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ICompanyRepository companyRepository)
    {
        _context = context;
        CategoryRepository = categoryRepository;
        ProductRepository = productRepository;
        CompanyRepository = companyRepository;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
