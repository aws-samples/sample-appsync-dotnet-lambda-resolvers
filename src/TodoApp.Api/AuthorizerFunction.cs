using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.AppSyncEvents;
using AWS.Lambda.Powertools.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApp.Api;

public class AuthorizerFunction
{
    [LambdaFunction]
    [Logging(LogEvent = true)]
    public Task<AppSyncAuthorizerResult> CustomLambdaAuthorizerHandler(AppSyncAuthorizerEvent appSyncAuthorizerEvent, ILambdaContext context)
    {
        Logger.LogInformation("Processing authorization request");
        
        var authorizationToken = appSyncAuthorizerEvent.AuthorizationToken;
        var apiId = appSyncAuthorizerEvent.RequestContext?.ApiId ?? "unknown";
        var accountId = appSyncAuthorizerEvent.RequestContext?.AccountId ?? "unknown";
        
        if (string.IsNullOrEmpty(authorizationToken))
        {
            Logger.LogError("Missing authorization token");
            return Task.FromResult(new AppSyncAuthorizerResult { IsAuthorized = false });
        }
        
        // CUSTOM LOGIC REQUIRED - Replace this simple token validation with your authentication logic
        // Examples: JWT validation, OAuth token verification, database user validation
        var isAuthorized = authorizationToken == "valid-token" || authorizationToken == "admin-token";
        Logger.LogInformation($"Authorization result: {isAuthorized}");
        
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
            // CUSTOM LOGIC OPTIONAL - Adjust TTL based on your security requirements
            TtlOverride = 300 // 5 minutes cache
        });
    }
}