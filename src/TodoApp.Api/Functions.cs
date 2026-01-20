using Amazon.Lambda.Annotations;
using Amazon.Lambda.AppSyncEvents;
using Amazon.Lambda.Core;
using AWS.Lambda.Powertools.Logging;
using TodoApp.Api.GraphQLTypes;
using TodoApp.Api.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TodoApp.Api
{
    public class Functions
    {
        private readonly ITodoService _todoService;

        public Functions(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [LambdaFunction()]
        [Logging(LogEvent = true)]
        public async Task<IEnumerable<Todo>> GetTodoItems()
        {
            return await _todoService.GetTodoItems();
        }

        [LambdaFunction()]
        [Logging(LogEvent = true)]
        public async Task<Todo> GetTodoItem(AppSyncResolverEvent<Dictionary<string, string>> appSyncEvent)
        {
            var id = appSyncEvent.Arguments["id"].ToString();
            return await _todoService.GetTodoItem(id);
        }

        [LambdaFunction()]
        [Logging(LogEvent = true)]
        public async Task<Todo> CreateTodoItem(AppSyncResolverEvent<CreateTodoItem> appSyncEvent)
        {
            return await _todoService.CreateTodoItem(appSyncEvent.Arguments);
        }

        [LambdaFunction]
        [Logging(LogEvent = true)]
        public async Task<Todo> UpdateTodoItem(AppSyncResolverEvent<UpdateTodoItem> appSyncEvent)
        {
            return await _todoService.UpdateTodoItem(appSyncEvent.Arguments);
        }

        [LambdaFunction]
        [Logging(LogEvent = true)]
        public async Task<Todo> DeleteTodoItem(AppSyncResolverEvent<Dictionary<string, string>> appSyncEvent)
        {
            // Check if user has admin role
            var role = appSyncEvent.RequestContext.Identity.Claims["role"];
            if (role != "admin")
            {
                throw new UnauthorizedAccessException("Only admins can delete todos");
            }

            var id = appSyncEvent.Arguments["id"].ToString();
            return await _todoService.DeleteTodoItem(id);
        }
    }
}

