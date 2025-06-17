#pragma warning disable CA2252

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAWSLambdaFunction<Projects.TodoApp_Api>("GetTodoItems",
       lambdaHandler: "TodoApp.Api::TodoApp.Api.Functions_GetTodoItems_Generated::GetTodoItems");
builder.AddAWSLambdaFunction<Projects.TodoApp_Api>("GetTodoItem",
       lambdaHandler: "TodoApp.Api::TodoApp.Api.Functions_GetTodoItem_Generated::GetTodoItem");
builder.AddAWSLambdaFunction<Projects.TodoApp_Api>("CreateTodoItem",
        lambdaHandler: "TodoApp.Api::TodoApp.Api.Functions_CreateTodoItem_Generated::CreateTodoItem");
builder.AddAWSLambdaFunction<Projects.TodoApp_Api>("UpdateTodoItem",
       lambdaHandler: "TodoApp.Api::TodoApp.Api.Functions_UpdateTodoItem_Generated::UpdateTodoItem");
builder.AddAWSLambdaFunction<Projects.TodoApp_Api>("DeleteTodoItem",
       lambdaHandler: "TodoApp.Api::TodoApp.Api.Functions_DeleteTodoItem_Generated::DeleteTodoItem");

builder.Build().Run();