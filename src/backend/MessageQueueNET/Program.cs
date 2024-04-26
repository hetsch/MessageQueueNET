using MessageQueueNET;
using MessageQueueNET.Client.Services.Abstraction;
using MessageQueueNET.Extensions;
using MessageQueueNET.Extensions.DependencyInjection;
using MessageQueueNET.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication
                    .CreateBuilder(args)
                    .Setup(args);

#if DEBUG
// Aspire
builder.AddServiceDefaults();
#endif

builder.Logging.AddConsole();
builder.Configuration.AddJsonFile(
            "_config/message-queue.json",
            optional: true,
            reloadOnChange: false
       );

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

#if DEBUG
// Aspire
app.MapDefaultEndpoints();
#endif

startup.Configure(
        app, 
        builder.Environment,
        app.Services.GetRequiredService<RestorePersistedQueuesService>(),
        app.Services.GetRequiredService<IMessageQueueApiVersionService>()
     );

app.LogStartupInformation(
        builder,
        app.Services.GetRequiredService<ILogger<Startup>>()
   )
   .Run();
