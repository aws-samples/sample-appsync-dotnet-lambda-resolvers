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
            
            if (string.IsNullOrEmpty(authorizationToken))
            {
                context.Logger.LogError("Missing authorization token");
                return Task.FromResult(new AppSyncAuthorizerResult { IsAuthorized = false });
            }
            
            // Simple validation
            var isAuthorized = authorizationToken == "valid-token" || authorizationToken == "admin-token";
            context.Logger.LogInformation($"Authorization result: {isAuthorized}");
            
            return Task.FromResult(new AppSyncAuthorizerResult
            {
                IsAuthorized = isAuthorized,
                ResolverContext = new Dictionary<string, string>
                {
                    { "userId", "user123" },
                    { "role", authorizationToken == "admin-token" ? "admin" : "user" },
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