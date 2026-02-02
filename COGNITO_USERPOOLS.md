# Cognito User Pools Authorization

## Deployment

Deploy with native Cognito User Pools authorization:

```bash
cd src/TodoApp.Api
dotnet lambda package
cd ../..
cdk deploy -c useCognitoUserPools=true
```

**Save these outputs:**
- `UserPoolId` - Your Cognito User Pool ID
- `UserPoolClientId` - Your Cognito Client ID
- `GraphQLAPIURL` - Your AppSync API endpoint

## Create Users

### Regular User
```bash
aws cognito-idp sign-up \
  --client-id <YOUR_CLIENT_ID> \
  --username testuser \
  --password <PASSWORD> \
  --user-attributes Name=email,Value=testuser@example.com

aws cognito-idp admin-confirm-sign-up \
  --user-pool-id <YOUR_USER_POOL_ID> \
  --username testuser
```

### Admin User
```bash
aws cognito-idp admin-create-user \
  --user-pool-id <YOUR_USER_POOL_ID> \
  --username adminuser \
  --user-attributes Name=email,Value=adminuser@example.com Name=email_verified,Value=true \
  --message-action SUPPRESS

aws cognito-idp admin-set-user-password \
  --user-pool-id <YOUR_USER_POOL_ID> \
  --username adminuser \
  --password <ADMIN_PASSWORD> \
  --permanent

aws cognito-idp admin-add-user-to-group \
  --user-pool-id <YOUR_USER_POOL_ID> \
  --username adminuser \
  --group-name Admins
```

## Get JWT Token

```bash
TOKEN=$(aws cognito-idp initiate-auth \
  --auth-flow USER_PASSWORD_AUTH \
  --client-id <YOUR_CLIENT_ID> \
  --auth-parameters USERNAME=testuser,PASSWORD=<PASSWORD> \
  --query 'AuthenticationResult.IdToken' \
  --output text)
```

## Test API

**Note:** Native Cognito User Pools does NOT use "Bearer" prefix

```bash
# Query
curl -X POST <YOUR_GRAPHQL_API_URL> \
  -H "Authorization: $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"query":"query { listTodos { id title } }"}'

# Create
curl -X POST <YOUR_GRAPHQL_API_URL> \
  -H "Authorization: $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"query":"mutation { createTodo(title: \"My Todo\", description: \"Test\") { id title } }"}'

# Delete (Admin only)
curl -X POST <YOUR_GRAPHQL_API_URL> \
  -H "Authorization: $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"query":"mutation { deleteTodo(id: \"<TODO_ID>\") { id } }"}'
```

## How It Works

1. User authenticates with Cognito → Gets JWT token
2. AppSync validates JWT automatically using Cognito User Pools
3. AppSync passes identity to Lambda resolvers
4. Delete resolver extracts `cognito:groups` from identity
5. Only users in Admins group can delete

## Permissions

| Operation | Regular User | Admin User |
|-----------|--------------|------------|
| listTodos | ✅ | ✅ |
| getTodoById | ✅ | ✅ |
| createTodo | ✅ | ✅ |
| updateTodo | ✅ | ✅ |
| deleteTodo | ❌ | ✅ |

## Comparison with Lambda Authorizer

| Feature | Native Cognito User Pools | Lambda Authorizer |
|---------|---------------------------|-------------------|
| **Deployment** | `-c useCognitoUserPools=true` | `-c useLambdaAuth=true` |
| **JWT Validation** | AppSync (automatic) | Custom Lambda function |
| **Authorization Header** | `Authorization: <token>` | `Authorization: Bearer <token>` |
| **Latency** | Lower | Higher (extra Lambda call) |
| **Cost** | Lower | Higher (Lambda invocations) |
| **Flexibility** | Standard Cognito | Custom logic possible |
| **Recommended** | ✅ Yes | For advanced use cases |
