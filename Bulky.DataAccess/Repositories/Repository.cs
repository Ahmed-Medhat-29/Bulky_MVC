using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bulky.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public T Get(Expression<Func<T, bool>> filter, string includeProperties = null, bool tracked = false)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            var properties = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
        }

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefault(filter);
    }

    public IEnumerable<T> GetList(Expression<Func<T, bool>> filter = null, string includeProperties = null)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            var properties = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
        }

        return query.ToList();
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
