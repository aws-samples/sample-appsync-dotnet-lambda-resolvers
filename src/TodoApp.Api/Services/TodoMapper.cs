
using TodoApp.Api.Entity;
using TodoApp.Api.GraphQLTypes;

namespace TodoApp.Api.Services
{
    public static class TodoMapper
    {
        public static Todo ToDto(TodoItemEntity entity)
        {
            if (entity == null)
            {
                return null!;
            }

            return new Todo
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Completed = entity.Completed,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}