using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public void Update(Company company)
    {
        _context.Companies.Update(company);
    }
}
