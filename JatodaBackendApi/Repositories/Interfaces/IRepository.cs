using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JatodaBackendApi.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T?>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> CreateAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}