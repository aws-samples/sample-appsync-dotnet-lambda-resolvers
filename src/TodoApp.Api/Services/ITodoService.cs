
using TodoApp.Api.GraphQLTypes;

namespace TodoApp.Api.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<Todo>> GetTodoItems();

        Task<Todo> GetTodoItem(string id);

        Task<Todo> CreateTodoItem(CreateTodoItem newTodoItem);

        Task<Todo> UpdateTodoItem(UpdateTodoItem updatedItem);

        Task<Todo> DeleteTodoItem(string id);
    }
}