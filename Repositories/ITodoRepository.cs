using TodoApp.Models;

namespace TodoApp.Repositories
{
    public interface ITodoRepository
    {
        Task<List<TodoItem>> GetAllAsync();
        Task<TodoItem?> GetByIdAsync(string id);
        Task CreateAsync(TodoItem item);
        Task UpdateAsync(string id, TodoItem item);
        Task DeleteAsync(string id);
    }
}
