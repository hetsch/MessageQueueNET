var builder = DistributedApplication.CreateBuilder(args);

var mq = builder
            .AddMessageQueueNET("messagequeue"/*, bridgeNetwork: "mq"*/)
            .WithBindMountPersistance();

var mqDashboard = builder
            .AddDashboardForMessageQueueNET("messagequeue-dashboard"/*, bridgeNetwork: "mq"*/)
            .ConnectToMessageQueue(mq, "mq")
            .ConnectToMessageQueue(mq, "mail", "mail*")
            .WithMaxPollingSeconds(5);

builder.Build().Run();
