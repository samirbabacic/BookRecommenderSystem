using BookRecommenderSystem.Entities;
using BookRecommenderSystem.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BookRecommenderSystem.Repository
{
    public interface IRepository<T> where T : BaseDocument
    {
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetPaged(int pageIndex, int pageSize);
        Task<T> GetByIdAsync(string id);
        Task CreateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteAsync(T entity);
        Task<bool> ReplaceAsync(string id, T entity);
        T FirstOrDefault(Expression<Func<T, bool>> expression);
        List<T> Get(Expression<Func<T, bool>> expression);
    }

    public class Repository<T> : IRepository<T> where T : BaseDocument
    {
        private readonly IMongoCollection<T> _collection;
        public Repository(IOptions<DbSettings> settings)
        {
            var database = new MongoClient(settings.Value.ConnectionString).GetDatabase(settings.Value.DatabaseName);
            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task CreateAsync(T model)
        {
            await _collection.InsertOneAsync(
                model,
                new InsertOneOptions { BypassDocumentValidation = false });
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq(document => document.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount == 1;
        }

        public async Task<bool> DeleteAsync(T model)
        {
            var result = await _collection.DeleteOneAsync(m => m.Id == model.Id);
            return result.DeletedCount == 1;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> expression)
        {
            return _collection.AsQueryable().Where(expression).FirstOrDefault();
        }

        public List<T> Get(Expression<Func<T, bool>> expression)
        {
            return _collection.AsQueryable().Where(expression).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var allDocuments = await _collection.FindAsync(condition => true);
            return allDocuments.ToEnumerable();
        }

        public async Task<bool> ReplaceAsync(string id, T model)
        {
            var filter = Builders<T>.Filter.Eq(document => document.Id, id);
            var result = await _collection.ReplaceOneAsync(filter, model, new ReplaceOptions { IsUpsert = false });
            return result.ModifiedCount > 0;
        }

        public IEnumerable<T> GetPaged(int pageIndex, int pageSize)
        {
            var pageDocuments = _collection.AsQueryable().Skip(pageIndex * pageSize).Take(pageSize);

            return pageDocuments;
        }
    }
}
