using Bulky.Models;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
	void Update(Category category);
}
