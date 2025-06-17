
using TodoApp.Api.Entity;

namespace TodoApp.Api.Repository
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoItemEntity>> GetTodoItems();
        Task<TodoItemEntity> GetTodoItem(string id);
        Task CreateTodoItem(TodoItemEntity todoItem);
        Task UpdateTodoItem(TodoItemEntity todoItem);
        Task DeleteTodoItem(TodoItemEntity todoItem);
    }
}