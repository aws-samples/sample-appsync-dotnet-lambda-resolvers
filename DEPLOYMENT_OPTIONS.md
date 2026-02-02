# Cognito Authentication - Deployment Options

## Two Implementation Approaches

### Option 1: Native Cognito User Pools (Recommended) ⭐

**Deploy:**
```bash
cdk deploy -c useLambdaAuth=true
```

**How it works:**
- AppSync validates JWT directly using Cognito User Pools
- No custom Lambda authorizer
- Identity available as `AppSyncCognitoIdentity` in resolvers

**Pros:**
- ✅ Simpler architecture
- ✅ Lower latency (no extra Lambda call)
- ✅ Lower cost (fewer Lambda invocations)
- ✅ AWS-recommended approach

**Authorization Header:**
```
Authorization: <JWT_TOKEN>
```
(No "Bearer" prefix)

---

### Option 2: Lambda Authorizer with Cognito JWT

**Deploy:**
```bash
cdk deploy -c useLambdaAuth=true -c useLambdaAuthorizer=true
```

**How it works:**
- Custom Lambda function validates JWT tokens
- Lambda extracts user info and groups
- Passes context to resolvers via `ResolverContext`
- Identity available as `AppSyncLambdaIdentity` in resolvers

**Pros:**
- ✅ Custom validation logic possible
- ✅ Can add additional checks beyond JWT
- ✅ Can integrate multiple auth sources

**Cons:**
- ❌ Extra Lambda invocation (cost + latency)
- ❌ More complex than needed for pure Cognito

**Authorization Header:**
```
Authorization: Bearer <JWT_TOKEN>
```
(Requires "Bearer" prefix)

---

## Code Compatibility

The resolver code (`Functions.cs`) automatically detects which mode is active:

```csharp
try
{
    // Try native Cognito User Pools first
    var cognitoIdentity = lambdaSerializer.Deserialize<AppSyncCognitoIdentity>(stream);
    // Extract groups from cognito:groups claim
}
catch
{
    // Fall back to Lambda Authorizer context
    var lambdaIdentity = lambdaSerializer.Deserialize<AppSyncLambdaIdentity>(stream);
    // Extract role from ResolverContext
}
```

Both modes support the same RBAC functionality - only Admins can delete todos.

---

## Which Should You Use?

**Use Option 1 (Native Cognito User Pools) if:**
- You only need Cognito authentication
- You want the simplest, most cost-effective solution
- You don't need custom validation logic

**Use Option 2 (Lambda Authorizer) if:**
- You need custom validation beyond JWT
- You want to add business logic to authorization
- You plan to support multiple auth sources
- You need to enrich the authorization context

---

## Switching Between Options

You can switch at any time by redeploying:

```bash
# Switch to Native Cognito User Pools
cdk deploy -c useLambdaAuth=true

# Switch to Lambda Authorizer
cdk deploy -c useLambdaAuth=true -c useLambdaAuthorizer=true
```

No code changes needed - the resolver automatically adapts!
