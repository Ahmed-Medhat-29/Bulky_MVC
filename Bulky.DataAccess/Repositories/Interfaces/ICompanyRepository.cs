using Bulky.Models;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface ICompanyRepository : IRepository<Company>
{
    void Update(Company company);
}
