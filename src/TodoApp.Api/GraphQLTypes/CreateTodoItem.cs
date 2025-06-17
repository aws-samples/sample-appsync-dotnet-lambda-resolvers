using System.Text.Json.Serialization;

namespace TodoApp.Api.GraphQLTypes
{
    public class CreateTodoItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
