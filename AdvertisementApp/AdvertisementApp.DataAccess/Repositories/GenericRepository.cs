using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdvertisementApp.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AdvertisementAppDbContext _context;

        public GenericRepository(AdvertisementAppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<T?> FindAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByFilterAsync(Expression<Func<T, bool>> filter, bool asNoTracking = false)
        {
            var query = _context.Set<T>().AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(filter);
        }

        public IQueryable<T> GetQuery()
        {
            return _context.Set<T>().AsQueryable();
        }

        public void Update(T entity, T unchangedEntity)
        {
            _context.Entry(unchangedEntity).CurrentValues.SetValues(entity);
        }
    }
}
