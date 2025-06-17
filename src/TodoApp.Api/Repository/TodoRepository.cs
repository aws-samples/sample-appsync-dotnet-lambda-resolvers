using Amazon.DynamoDBv2.DataModel;
using TodoApp.Api.Entity;

namespace TodoApp.Api.Repository
{
    public class TodoRepository : ITodoRepository
    {
        private readonly IDynamoDBContext _dynamoDBContext;

        public TodoRepository(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
        }

        public async Task<IEnumerable<TodoItemEntity>> GetTodoItems()
        {
            var search = _dynamoDBContext.ScanAsync<TodoItemEntity>(new List<ScanCondition>());
            return await search.GetRemainingAsync();
        }

        public async Task<TodoItemEntity> GetTodoItem(string id)
        {
            var todoItem = await _dynamoDBContext.LoadAsync<TodoItemEntity>(id);
            return todoItem;
        }

        public async Task CreateTodoItem(TodoItemEntity todoItem)
        {
            await _dynamoDBContext.SaveAsync(todoItem);
        }

        public async Task UpdateTodoItem(TodoItemEntity todoItem)
        {
            await _dynamoDBContext.SaveAsync(todoItem);
        }

        public async Task DeleteTodoItem(TodoItemEntity todoItem)
        {
            await _dynamoDBContext.DeleteAsync(todoItem);
        }
    }
}