using MessageQueueNET.Client.Services;
using MessageQueueNET.Client.Services.Abstraction;
using MessageQueueNET.Core.Services.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
using System.Security.Authentication;

namespace MessageQueueNET.Client.Extensions.DependencyInjetion;

static public class ServiceCollectionExtensoins
{
    static public IServiceCollection AddMessageQueueAppVersionService(this IServiceCollection services)
    {
        services.TryAddSingleton<IMessageQueueApiVersionService, MessageQueueApiVersionService>();

        return services;
    }

    static public IServiceCollection AddMessageQueueClientService(this IServiceCollection services)
    {
        services
            .AddHttpClient(MessageQueueClientService.HttpClientName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(90);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12 | /*SslProtocols.Tls13 |*/ SslProtocols.Tls
                //,ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });

        return services
            .AddMessageQueueAppVersionService()
            .AddTransient<MessageQueueClientService>();
    }

    static public IServiceCollection AddQueueWatcher(this IServiceCollection services,
                                                     Action<QueueWatcherBackgroundServiceOptions> configAction)
    {
        return services
            .Configure(configAction)
            .AddMessageQueueClientService()
            .AddTransient<IQueueProcessor, PingWorker>()
            .AddHostedService<QueueWatcherBackgroundService>();
    }
}
