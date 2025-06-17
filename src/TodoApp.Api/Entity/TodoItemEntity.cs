using Amazon.DynamoDBv2.DataModel;

namespace TodoApp.Api.Entity
{
    [DynamoDBTable("TodoItems")]
    public class TodoItemEntity
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool Completed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
