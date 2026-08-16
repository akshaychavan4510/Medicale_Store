using System.Linq.Expressions;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    using System.Linq.Expressions;

    namespace MedicalStore.Repository.Interfaces
    {
        public interface IGenericRepository<T> where T : class
        {
            Task<IEnumerable<T>> GetAllAsync();
            Task<T?> GetByIdAsync(int id);
            Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
            Task AddAsync(T entity);
            void Update(T entity);
            void Delete(T entity);
            IQueryable<T> GetQueryable();    // ← add here once; all repos inherit it
        }
    }
}

