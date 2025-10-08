using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
	IEnumerable<T> GetList();
	T Get(Expression<Func<T, bool>> filter);
	void Add(T entity);
	void Remove(T entity);
	void RemoveRange(IEnumerable<T> entities);
}
