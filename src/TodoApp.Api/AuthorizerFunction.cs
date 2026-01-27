using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.AppSyncEvents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApp.Api;

public class AuthorizerFunction
{
    [LambdaFunction]
    public Task<AppSyncAuthorizerResult> CustomLambdaAuthorizerHandler(AppSyncAuthorizerEvent appSyncAuthorizerEvent, ILambdaContext context)
    {
        context.Logger.LogInformation("Processing authorization request");
        
        try
        {
            var authorizationToken = appSyncAuthorizerEvent.AuthorizationToken;
            var apiId = appSyncAuthorizerEvent.RequestContext.ApiId;
            var accountId = appSyncAuthorizerEvent.RequestContext.AccountId;
            
            if (string.IsNullOrEmpty(authorizationToken) || !authorizationToken.StartsWith("Bearer "))
            {
                context.Logger.LogError("Invalid token format");
                return Task.FromResult(new AppSyncAuthorizerResult { IsAuthorized = false });
            }

            var actualToken = authorizationToken.Substring("Bearer ".Length);
            
            // Simple validation
            var isAuthorized = actualToken == "valid-token" || actualToken == "admin-token";
            context.Logger.LogInformation($"Authorization result: {isAuthorized}");
            
            return Task.FromResult(new AppSyncAuthorizerResult
            {
                IsAuthorized = isAuthorized,
                ResolverContext = new Dictionary<string, string>
                {
                    { "userId", "user123" },
                    { "role", actualToken == "admin-token" ? "admin" : "user" },
                    { "apiId", apiId },
                    { "accountId", accountId }
                },
                TtlOverride = 300 // 5 minutes cache
            });
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Authorization error: {ex.Message}");
            return Task.FromResult(new AppSyncAuthorizerResult { IsAuthorized = false });
        }
    }
}