

using TodoApp.Api.Entity;
using TodoApp.Api.Repository;
using TodoApp.Api.GraphQLTypes;

namespace TodoApp.Api.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;

        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<IEnumerable<Todo>> GetTodoItems()
        {
            var entities = await _todoRepository.GetTodoItems();
            return entities.Select(TodoMapper.ToDto);
        }

        public async Task<Todo> GetTodoItem(string id)
        {
            var entity = await _todoRepository.GetTodoItem(id);
            return TodoMapper.ToDto(entity); 
        }

        public async Task<Todo> CreateTodoItem(CreateTodoItem newItem)
        {
            var entity = new TodoItemEntity
            {
                Id = Guid.NewGuid().ToString(),
                Title = newItem.Title,
                Description = newItem.Description,
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _todoRepository.CreateTodoItem(entity);
            return TodoMapper.ToDto(entity);
        }

        public async Task<Todo> UpdateTodoItem(UpdateTodoItem updatedItem)
        {
            var entity = await _todoRepository.GetTodoItem(updatedItem.Id) ?? throw new Exception("Todo item not found");
            entity.Title = updatedItem.Title;
            entity.Description = updatedItem.Description;
            entity.Completed = updatedItem.Completed;
            entity.UpdatedAt = DateTime.UtcNow;
            await _todoRepository.UpdateTodoItem(entity);
            return TodoMapper.ToDto(entity);
        }

        public async Task<Todo> DeleteTodoItem(string id)
        {
            var entity = await _todoRepository.GetTodoItem(id) ?? throw new Exception("Todo item not found");
            await _todoRepository.DeleteTodoItem(entity);
            return TodoMapper.ToDto(entity);
        }
    }
}