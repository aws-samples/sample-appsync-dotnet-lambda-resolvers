using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.IAM;
using Constructs;
using System.Collections.Generic;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace TodoApp.CDK
{
    public class AppSyncApiStack : Stack
    {
        private FunctionProps CreateLambdaProps(string handler, string tableName)
        {
            return new FunctionProps
            {
                Runtime = Runtime.DOTNET_8,
                Code = Amazon.CDK.AWS.Lambda.Code.FromAsset("src/TodoApp.Api/bin/Release/net8.0/publish"),
                Handler = handler,
                MemorySize = 256,
                Timeout = Duration.Seconds(30),
                Environment = new Dictionary<string, string>
                {
                    { "DYNAMODB_TABLE", tableName },
                    { "POWERTOOLS_SERVICE_NAME", "TodoApp" },
                    { "POWERTOOLS_METRICS_NAMESPACE", "TodoApp" }
                }
            };
        }

        public AppSyncApiStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Check if Lambda authorization is enabled via context
            var useLambdaAuth = this.Node.TryGetContext("useLambdaAuth")?.ToString() == "true";

            // Create DynamoDB Table
            var todoTable = new Table(this, "TodoTable", new TableProps
            {
                TableName = "TodoItems", // This should match your [DynamoDBTable] attribute
                PartitionKey = new Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create authorization config based on context
            AuthorizationConfig authConfig;
            Function authorizerFunction = null;

            if (useLambdaAuth)
            {
                // Create Lambda Authorizer Function
                authorizerFunction = new Function(this, "AppSyncAuthorizerFunction", new FunctionProps
                {
                    Runtime = Runtime.DOTNET_8,
                    Code = Amazon.CDK.AWS.Lambda.Code.FromAsset("src/TodoApp.Api/bin/Release/net8.0/publish"),
                    Handler = "TodoApp.Api::TodoApp.Api.AuthorizerFunction_CustomLambdaAuthorizerHandler_Generated::CustomLambdaAuthorizerHandler",
                    MemorySize = 256,
                    Timeout = Duration.Seconds(30),
                    Environment = new Dictionary<string, string>
                    {
                        { "POWERTOOLS_SERVICE_NAME", "TodoAppAuthorizer" },
                        { "POWERTOOLS_METRICS_NAMESPACE", "TodoApp" }
                    }
                });

                authConfig = new AuthorizationConfig
                {
                    DefaultAuthorization = new AuthorizationMode
                    {
                        AuthorizationType = AuthorizationType.LAMBDA,
                        LambdaAuthorizerConfig = new LambdaAuthorizerConfig
                        {
                            Handler = authorizerFunction,
                            ResultsCacheTtl = Duration.Minutes(5)
                        }
                    }
                };
            }
            else
            {
                authConfig = new AuthorizationConfig
                {
                    DefaultAuthorization = new AuthorizationMode
                    {
                        AuthorizationType = AuthorizationType.API_KEY,
                        ApiKeyConfig = new ApiKeyConfig
                        {
                            Expires = Expiration.After(Duration.Days(30))
                        }
                    }
                };
            }

            // Create AppSync API
            var api = new GraphqlApi(this, "TodoApi", new GraphqlApiProps
            {
                Name = "dotnet-appsync-todo-api",
                Definition = Definition.FromFile("src/TodoApp.Cdk/graphql/schema.graphql"),
                AuthorizationConfig = authConfig,
                XrayEnabled = true
            });

            // Create Lambda Functions with specific permissions
            var getTodoItemsFunction = new Function(this, "GetTodoItemsFunction",
                CreateLambdaProps("TodoApp.Api::TodoApp.Api.Functions_GetTodoItems_Generated::GetTodoItems", todoTable.TableName));
            todoTable.GrantReadData(getTodoItemsFunction);
           

            var getTodoItemFunction = new Function(this, "GetTodoItemFunction",
                CreateLambdaProps("TodoApp.Api::TodoApp.Api.Functions_GetTodoItem_Generated::GetTodoItem", todoTable.TableName));
            todoTable.GrantReadData(getTodoItemFunction);

            var createTodoItemFunction = new Function(this, "CreateTodoItemFunction",
                CreateLambdaProps("TodoApp.Api::TodoApp.Api.Functions_CreateTodoItem_Generated::CreateTodoItem", todoTable.TableName));
            todoTable.GrantReadWriteData(createTodoItemFunction);

            var updateTodoItemFunction = new Function(this, "UpdateTodoItemFunction",
                CreateLambdaProps("TodoApp.Api::TodoApp.Api.Functions_UpdateTodoItem_Generated::UpdateTodoItem", todoTable.TableName));
            todoTable.GrantReadWriteData(updateTodoItemFunction);


            var deleteTodoItemFunction = new Function(this, "DeleteTodoItemFunction",
                CreateLambdaProps("TodoApp.Api::TodoApp.Api.Functions_DeleteTodoItem_Generated::DeleteTodoItem", todoTable.TableName));
            todoTable.GrantReadWriteData(deleteTodoItemFunction);

            // Create AppSync DataSources
            var getTodoItemsDataSource = api.AddLambdaDataSource("GetTodoItemsDataSource", getTodoItemsFunction);
            var getTodoItemByIdDataSource = api.AddLambdaDataSource("GetTodoItemByIdDataSource", getTodoItemFunction);
            var createTodoItemDataSource = api.AddLambdaDataSource("CreateTodoItemDataSource", createTodoItemFunction);
            var updateTodoItemDataSource = api.AddLambdaDataSource("UpdateTodoItemDataSource", updateTodoItemFunction);
            var deleteTodoItemDataSource = api.AddLambdaDataSource("DeleteTodoItemDataSource", deleteTodoItemFunction);

            // Add Direct Lambda Resolvers
            getTodoItemsDataSource.CreateResolver("ListTodosResolver", new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "listTodos"
            });

            getTodoItemByIdDataSource.CreateResolver("GetTodoByIdResolver", new BaseResolverProps
            {
                TypeName = "Query",
                FieldName = "getTodoById"
            });

            createTodoItemDataSource.CreateResolver("CreateTodoResolver", new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "createTodo"
            });

            updateTodoItemDataSource.CreateResolver("UpdateTodoResolver", new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "updateTodo"
            });

            deleteTodoItemDataSource.CreateResolver("DeleteTodoResolver", new BaseResolverProps
            {
                TypeName = "Mutation",
                FieldName = "deleteTodo"
            });

            // Output the AppSync API URL
            new CfnOutput(this, "GraphQLAPIURL", new CfnOutputProps
            {
                Value = api.GraphqlUrl
            });

            // Conditional outputs based on auth type
            if (useLambdaAuth)
            {
                new CfnOutput(this, "AuthorizationType", new CfnOutputProps
                {
                    Value = "Lambda Authorizer",
                    Description = "Use 'valid-token' in Authorization header"
                });

                new CfnOutput(this, "AuthorizerFunctionArn", new CfnOutputProps
                {
                    Value = authorizerFunction!.FunctionArn
                });
            }
            else
            {
                new CfnOutput(this, "GraphQLAPIKey", new CfnOutputProps
                {
                    Value = api.ApiKey ?? "No API Key"
                });

                new CfnOutput(this, "AuthorizationType", new CfnOutputProps
                {
                    Value = "API Key",
                    Description = "Use x-api-key header with the API key"
                });
            }
        }
    }
}