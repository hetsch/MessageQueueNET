using Microsoft.Extensions.Hosting;

namespace MessageQueueNET.Extensions.DependencyInjection;

static public class WebApplicationBuilderExtensions
{
    static public TBuilder Setup<TBuilder>(this TBuilder builder, string[] args)
        where TBuilder : IHostApplicationBuilder
    {
        #region First Start => init configuration

        new Setup().TrySetup(args);

        #endregion

        return builder;
    }
}
