var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.MessageQueueNET>("messagequeuenet");

builder.AddProject<Projects.MessageQueueNET_Blazor>("messagequeuenet-blazor")
    .WithReference(api);

builder.Build().Run();
