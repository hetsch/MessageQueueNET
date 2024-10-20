var builder = DistributedApplication.CreateBuilder(args);

var mq = builder
            .AddMessageQueueNET("messagequeue")
            .WithBindMountPersistance();

var mqDashboard = builder
            .AddDashboardForMessageQueueNET("messagequeue-dashboard")
            .ConnectToMessageQueue(mq, "mq")
            .ConnectToMessageQueue(mq, "mail", "mail*")
            .WithMaxPollingSeconds(5)
            .Build();

builder.Build().Run();
