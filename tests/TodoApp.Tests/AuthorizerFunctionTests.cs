using Amazon.Lambda.AppSyncEvents;
using Amazon.Lambda.Core;
using Moq;
using TodoApp.Api;

namespace TodoApp.Tests
{
    public class AuthorizerFunctionTests
    {
        private readonly AuthorizerFunction _function;
        private readonly Mock<ILambdaContext> _mockContext;

        public AuthorizerFunctionTests()
        {
            _function = new AuthorizerFunction();
            _mockContext = new Mock<ILambdaContext>();
        }

        [Fact]
        public async Task CustomLambdaAuthorizerHandler_ReturnsAuthorized_WithValidToken()
        {
            // Arrange
            var authEvent = new AppSyncAuthorizerEvent
            {
                AuthorizationToken = "valid-token"
            };

            // Act
            var result = await _function.CustomLambdaAuthorizerHandler(authEvent, _mockContext.Object);

            // Assert
            Assert.True(result.IsAuthorized);
            Assert.NotNull(result.ResolverContext);
            Assert.Equal("user123", result.ResolverContext["userId"]);
            Assert.Equal("user", result.ResolverContext["role"]);
            Assert.Equal(300, result.TtlOverride);
        }

        [Fact]
        public async Task CustomLambdaAuthorizerHandler_ReturnsAuthorized_WithAdminToken()
        {
            // Arrange
            var authEvent = new AppSyncAuthorizerEvent
            {
                AuthorizationToken = "admin-token"
            };

            // Act
            var result = await _function.CustomLambdaAuthorizerHandler(authEvent, _mockContext.Object);

            // Assert
            Assert.True(result.IsAuthorized);
            Assert.NotNull(result.ResolverContext);
            Assert.Equal("admin", result.ResolverContext["role"]);
            Assert.Equal(300, result.TtlOverride);
        }

        [Fact]
        public async Task CustomLambdaAuthorizerHandler_ReturnsUnauthorized_WithInvalidToken()
        {
            // Arrange
            var authEvent = new AppSyncAuthorizerEvent
            {
                AuthorizationToken = "invalid-token"
            };

            // Act
            var result = await _function.CustomLambdaAuthorizerHandler(authEvent, _mockContext.Object);

            // Assert
            Assert.False(result.IsAuthorized);
        }

        [Fact]
        public async Task CustomLambdaAuthorizerHandler_ReturnsUnauthorized_WithEmptyToken()
        {
            // Arrange
            var authEvent = new AppSyncAuthorizerEvent
            {
                AuthorizationToken = ""
            };

            // Act
            var result = await _function.CustomLambdaAuthorizerHandler(authEvent, _mockContext.Object);

            // Assert
            Assert.False(result.IsAuthorized);
        }

        [Fact]
        public async Task CustomLambdaAuthorizerHandler_ReturnsUnauthorized_WithNullToken()
        {
            // Arrange
            var authEvent = new AppSyncAuthorizerEvent
            {
                AuthorizationToken = null
            };

            // Act
            var result = await _function.CustomLambdaAuthorizerHandler(authEvent, _mockContext.Object);

            // Assert
            Assert.False(result.IsAuthorized);
        }
    }
}
