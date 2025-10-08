using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using System;

namespace Bulky.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _dbContext;

	public ICategoryRepository CategoryRepository { get; set; }

	public UnitOfWork(ApplicationDbContext dbContext, ICategoryRepository categoryRepository)
	{
		_dbContext = dbContext;
		CategoryRepository = categoryRepository;
	}

	public void Save()
	{
		_dbContext.SaveChanges();
	}
}
