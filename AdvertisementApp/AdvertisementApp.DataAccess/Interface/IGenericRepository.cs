using System.Linq.Expressions;

namespace AdvertisementApp.DataAccess.Interface
{
    public interface IGenericRepository<T>
    {
        Task<List<T>> GetAllAsync(); 
        Task<T?> FindAsync(int id); 
        Task<T?> GetByFilterAsync(Expression<Func<T, bool>> filter, bool asNoTracking = false); 
        Task CreateAsync(T entity); void Update(T entity, T unchangedEntity); 
        void Delete(T entity); 
        IQueryable<T> GetQuery();
    }
}
