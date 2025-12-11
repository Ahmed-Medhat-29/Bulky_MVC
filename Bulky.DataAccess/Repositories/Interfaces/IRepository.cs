using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetList(Expression<Func<T, bool>> filter = null, string includeProperties = null);
    T Get(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false);
    int Count(Expression<Func<T, bool>> filter);
    void Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
