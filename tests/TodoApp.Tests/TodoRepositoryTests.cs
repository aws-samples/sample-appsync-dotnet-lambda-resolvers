using Amazon.DynamoDBv2.DataModel;
using Moq;
using TodoApp.Api.Entity;
using TodoApp.Api.Repository;

namespace TodoApp.Tests
{
    public class TodoRepositoryTests
    {
        private readonly Mock<IDynamoDBContext> _mockContext;
        private readonly TodoRepository _repository;

        public TodoRepositoryTests()
        {
            _mockContext = new Mock<IDynamoDBContext>();
            _repository = new TodoRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetTodoItems_ReturnsAllItems()
        {
            // Arrange
            var items = new List<TodoItemEntity>
            {
                new TodoItemEntity { Id = "1", Title = "Test 1" },
                new TodoItemEntity { Id = "2", Title = "Test 2" }
            };

            var mockAsyncSearch = new Mock<AsyncSearch<TodoItemEntity>>();

            _mockContext.Setup(c => c.ScanAsync<TodoItemEntity>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()))
                        .Returns(mockAsyncSearch.Object);

            mockAsyncSearch.Setup(s => s.GetRemainingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(items);

            // Act
            var result = await _repository.GetTodoItems();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.Id == "1");
            Assert.Contains(result, i => i.Id == "2");
            _mockContext.Verify(c => c.ScanAsync<TodoItemEntity>(It.IsAny<List<ScanCondition>>(), It.IsAny<DynamoDBOperationConfig>()), Times.Once);
            mockAsyncSearch.Verify(s => s.GetRemainingAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsItem_WhenExists()
        {
            // Arrange
            var item = new TodoItemEntity { Id = "1", Title = "Test" };
            _mockContext.Setup(c => c.LoadAsync<TodoItemEntity>("1", default)).ReturnsAsync(item);

            // Act
            var result = await _repository.GetTodoItem("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            _mockContext.Verify(c => c.LoadAsync<TodoItemEntity>("1", default), Times.Once);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _mockContext.Setup(c => c.LoadAsync<TodoItemEntity>("1", default)).ReturnsAsync((TodoItemEntity)null);

            // Act
            var result = await _repository.GetTodoItem("1");

            // Assert
            Assert.Null(result);
            _mockContext.Verify(c => c.LoadAsync<TodoItemEntity>("1", default), Times.Once);
        }

        [Fact]
        public async Task CreateTodoItem_SavesItem()
        {
            // Arrange
            var item = new TodoItemEntity { Id = "1", Title = "Test" };
            _mockContext.Setup(c => c.SaveAsync(item, default)).Returns(Task.CompletedTask);

            // Act
            await _repository.CreateTodoItem(item);

            // Assert
            _mockContext.Verify(c => c.SaveAsync(item, default), Times.Once);
        }

        [Fact]
        public async Task UpdateTodoItem_SavesItem()
        {
            // Arrange
            var item = new TodoItemEntity { Id = "1", Title = "Updated" };
            _mockContext.Setup(c => c.SaveAsync(item, default)).Returns(Task.CompletedTask);

            // Act
            await _repository.UpdateTodoItem(item);

            // Assert
            _mockContext.Verify(c => c.SaveAsync(item, default), Times.Once);
        }

        [Fact]
        public async Task DeleteTodoItem_DeletesItem()
        {
            // Arrange
            var item = new TodoItemEntity { Id = "1", Title = "Test" };
            _mockContext.Setup(c => c.DeleteAsync(item, default)).Returns(Task.CompletedTask);

            // Act
            await _repository.DeleteTodoItem(item);

            // Assert
            _mockContext.Verify(c => c.DeleteAsync(item, default), Times.Once);
        }
    }
}