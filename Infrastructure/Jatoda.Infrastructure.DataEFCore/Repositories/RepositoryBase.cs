using System.Linq.Expressions;
using Jatoda.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jatoda.Infrastructure.DataEFCore.Repositories;

public abstract class RepositoryBase<T>(JatodaContext repositoryContext)
    : IRepositoryBase<T>
    where T : class
{
    public virtual IQueryable<T> FindAll(bool trackChanges)
    {
        return !trackChanges
            ? repositoryContext.Set<T>()
                .AsNoTracking()
            : repositoryContext.Set<T>();
    }

    public virtual IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression,
        bool trackChanges)
    {
        return !trackChanges
            ? repositoryContext.Set<T>()
                .Where(expression)
                .AsNoTracking()
            : repositoryContext.Set<T>()
                .Where(expression);
    }

    public virtual void Create(T entity)
    {
        repositoryContext.Set<T>().Add(entity);
    }

    public virtual void Update(T entity)
    {
        repositoryContext.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        repositoryContext.Set<T>().Remove(entity);
    }
}