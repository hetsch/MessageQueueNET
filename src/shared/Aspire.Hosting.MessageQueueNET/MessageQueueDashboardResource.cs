namespace Aspire.Hosting.ApplicationModel;

public class MessageQueueDashboardResource(string name, string? bridgeNetwork = null)
    : ContainerResource(name)
{
    internal const string HttpEndpointName = "http";

    public int ContainerHttpPort = 8080;

    private EndpointReference? _httpReference;

    internal string? BridgeNetwork { get; set; } = bridgeNetwork;

    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);
}
