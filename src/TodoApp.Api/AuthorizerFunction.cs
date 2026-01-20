using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using System.Text.Json;

namespace TodoApp.Api;

public class AuthorizerFunction
{
    [LambdaFunction]
    public Dictionary<string, object> Authorize(Dictionary<string, object> request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Authorization request: {JsonSerializer.Serialize(request)}");

        try
        {
            // Get the authorization token from the request
            if (!request.TryGetValue("authorizationToken", out var tokenObj))
            {
                context.Logger.LogError("No authorizationToken found");
                return new Dictionary<string, object> { { "isAuthorized", false } };
            }

            var token = tokenObj?.ToString() ?? "";
            context.Logger.LogInformation($"Token: {token}");
            
            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
            {
                context.Logger.LogError("Invalid token format");
                return new Dictionary<string, object> { { "isAuthorized", false } };
            }

            var actualToken = token.Substring("Bearer ".Length);
            context.Logger.LogInformation($"Actual token: {actualToken}");
            
            // Simple validation
            var isAuthorized = actualToken == "valid-token" || actualToken == "admin-token";
            context.Logger.LogInformation($"Is authorized: {isAuthorized}");
            
            return new Dictionary<string, object>
            {
                { "isAuthorized", isAuthorized },
                { "resolverContext", new Dictionary<string, object>
                    {
                        { "userId", "user123" },
                        { "role", actualToken == "admin-token" ? "admin" : "user" }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex.Message}");
            return new Dictionary<string, object> { { "isAuthorized", false } };
        }
    }
}