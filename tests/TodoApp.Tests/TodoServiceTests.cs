using Moq;
using TodoApp.Api.Entity;
using TodoApp.Api.Repository;
using TodoApp.Api.Services;
using TodoApp.Api.GraphQLTypes;

namespace TodoApp.Tests
{
    public class TodoServiceTests
    {
        private readonly Mock<ITodoRepository> _mockRepo;
        private readonly TodoService _service;

        public TodoServiceTests()
        {
            _mockRepo = new Mock<ITodoRepository>();
            _service = new TodoService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetTodoItems_ReturnsAllItems()
        {
            // Arrange
            var entities = new List<TodoItemEntity>
            {
                new TodoItemEntity { Id = "1", Title = "Test 1", Completed = false },
                new TodoItemEntity { Id = "2", Title = "Test 2", Completed = true }
            };
            _mockRepo.Setup(r => r.GetTodoItems()).ReturnsAsync(entities);

            // Act
            var result = await _service.GetTodoItems();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.Id == "1");
            Assert.Contains(result, i => i.Id == "2");
            _mockRepo.Verify(r => r.GetTodoItems(), Times.Once);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsItem_WhenExists()
        {
            // Arrange
            var entity = new TodoItemEntity { Id = "1", Title = "Test", Completed = false };
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync(entity);

            // Act
            var result = await _service.GetTodoItem("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Test", result.Title);
            _mockRepo.Verify(r => r.GetTodoItem("1"), Times.Once);
        }

        [Fact]
        public async Task GetTodoItem_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync((TodoItemEntity)null);

            // Act
            var result = await _service.GetTodoItem("1");

            // Assert
            Assert.Null(result);
            _mockRepo.Verify(r => r.GetTodoItem("1"), Times.Once);
        }

        [Fact]
        public async Task CreateTodoItem_CreatesNewItem()
        {
            // Arrange
            var newItem = new CreateTodoItem { Title = "New Todo", Description = "Test Desc" };
            _mockRepo.Setup(r => r.CreateTodoItem(It.IsAny<TodoItemEntity>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateTodoItem(newItem);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Id));
            Assert.Equal("New Todo", result.Title);
            Assert.Equal("Test Desc", result.Description);
            Assert.False(result.Completed);
            Assert.True(result.CreatedAt > DateTime.MinValue);
            _mockRepo.Verify(r => r.CreateTodoItem(It.Is<TodoItemEntity>(i => i.Title == "New Todo")), Times.Once);
        }

        [Fact]
        public async Task UpdateTodoItem_UpdatesExistingItem()
        {
            // Arrange
            var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
            var existingEntity = new TodoItemEntity
            {
                Id = "1",
                Title = "Old",
                Description = "Old Desc",
                Completed = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = originalUpdatedAt
            };
            var updatedItem = new UpdateTodoItem
            {
                Id = "1",
                Title = "New",
                Description = "New Desc",
                Completed = true
            };
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync(existingEntity);

            TodoItemEntity capturedEntity = null;
            _mockRepo.Setup(r => r.UpdateTodoItem(It.IsAny<TodoItemEntity>()))
                     .Callback<TodoItemEntity>(e => capturedEntity = e) 
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateTodoItem(updatedItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("New", result.Title);
            Assert.Equal("New Desc", result.Description);
            Assert.True(result.Completed);
            Assert.True(result.UpdatedAt > originalUpdatedAt); 
            Assert.NotNull(capturedEntity); 
            Assert.Equal("New", capturedEntity.Title);
            Assert.Equal("New Desc", capturedEntity.Description);
            Assert.True(capturedEntity.Completed);
            Assert.True(capturedEntity.UpdatedAt > originalUpdatedAt); 
            _mockRepo.Verify(r => r.GetTodoItem("1"), Times.Once);
            _mockRepo.Verify(r => r.UpdateTodoItem(It.Is<TodoItemEntity>(i => i.Title == "New" && i.Completed)), Times.Once);
        }

        [Fact]
        public async Task UpdateTodoItem_Throws_WhenNotFound()
        {
            // Arrange
            var updatedItem = new UpdateTodoItem { Id = "1", Title = "New", Description = "New Desc", Completed = true };
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync((TodoItemEntity)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateTodoItem(updatedItem));
            _mockRepo.Verify(r => r.UpdateTodoItem(It.IsAny<TodoItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTodoItem_DeletesItem_WhenExists()
        {
            // Arrange
            var entity = new TodoItemEntity { Id = "1", Title = "Test" };
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.DeleteTodoItem(entity)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteTodoItem("1");

            // Assert
            Assert.Equal("1", result.Id);
            _mockRepo.Verify(r => r.DeleteTodoItem(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteTodoItem_Throws_WhenNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetTodoItem("1")).ReturnsAsync((TodoItemEntity)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteTodoItem("1"));
            _mockRepo.Verify(r => r.DeleteTodoItem(It.IsAny<TodoItemEntity>()), Times.Never);
        }
    }
}