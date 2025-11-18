using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Bulky.Models;

namespace Bulky.DataAccess.Repositories;

public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
{
    public ApplicationUserRepository(ApplicationDbContext context) : base(context)
    {
    }
}
