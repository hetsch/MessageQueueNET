namespace Aspire.Hosting.ApplicationModel;

public class MessageQueueResource(string name, string? bridgeNetwork = null) 
    : ContainerResource(name)
{
    internal const string HttpsEndpointName = "https";
    internal const string HttpEndpointName = "http";

    internal string? BridgeNetwork { get; set; } = bridgeNetwork;

    private EndpointReference? _httpReference;
    private EndpointReference? _httpsReference;

    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);

    public EndpointReference HttpsEndpoint =>
        _httpsReference ??= new(this, HttpsEndpointName);
}
