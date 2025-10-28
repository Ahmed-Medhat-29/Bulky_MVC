using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IEnumerable<T> GetList(string includeProperties = null);
    T Get(Expression<Func<T, bool>> filter, string includeProperties = null);
    void Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
