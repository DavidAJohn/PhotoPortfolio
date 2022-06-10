using System.Linq.Expressions;

namespace PhotoPortfolio.Server.Contracts;

public interface IBaseRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T obj);
    Task<T> UpdateAsync(T obj);
    Task DeleteAsync(string id);
}
