using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TodoApp.Models;

namespace TodoApp.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly IMongoCollection<TodoItem> _collection;

        public TodoRepository(IMongoClient client, IOptions<MongoSettings> settings)
        {
            var db = client.GetDatabase(settings.Value.DatabaseName);
            _collection = db.GetCollection<TodoItem>(settings.Value.CollectionName);
        }

        public async Task<List<TodoItem>> GetAllAsync()
        {
            return await _collection.Find(_ => true).SortByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAsync(string id)
        {
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(TodoItem item)
        {
            await _collection.InsertOneAsync(item);
        }

        public async Task UpdateAsync(string id, TodoItem item)
        {
            await _collection.ReplaceOneAsync(t => t.Id == id, item);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(t => t.Id == id);
        }
    }
}
