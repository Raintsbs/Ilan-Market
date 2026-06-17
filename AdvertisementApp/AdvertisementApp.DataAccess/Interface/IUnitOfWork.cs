namespace AdvertisementApp.DataAccess.Interface
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> GetRepository<T>() where T : class; 
        Task SaveChanges();
    }
}
