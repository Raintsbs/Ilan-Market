using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.DataAccess.Interface;
using AdvertisementApp.DataAccess.Repositories;

namespace AdvertisementApp.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AdvertisementAppDbContext _context;

        public UnitOfWork(AdvertisementAppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
