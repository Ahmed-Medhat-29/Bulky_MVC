using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public ICategoryRepository Category { get; private set; }
    public IProductRepository Product { get; private set; }
    public ICompanyRepository Company { get; private set; }
    public IShoppingCartRepository ShoppingCart { get; private set; }
    public IApplicationUserRepository ApplicationUser { get; private set; }
    public IOrderHeaderRepository OrderHeader { get; private set; }
    public IOrderDetailRepository OrderDetail { get; private set; }

    public UnitOfWork(ApplicationDbContext context,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ICompanyRepository companyRepository,
        IShoppingCartRepository shoppingCartRepository,
        IApplicationUserRepository applicationUserRepository,
        IOrderHeaderRepository orderHeader,
        IOrderDetailRepository orderDetail)
    {
        _context = context;
        Category = categoryRepository;
        Product = productRepository;
        Company = companyRepository;
        ShoppingCart = shoppingCartRepository;
        ApplicationUser = applicationUserRepository;
        OrderHeader = orderHeader;
        OrderDetail = orderDetail;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
