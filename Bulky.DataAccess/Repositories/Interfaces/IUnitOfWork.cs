namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IUnitOfWork
{
	ICategoryRepository CategoryRepository { get; }
	void Save();
}
