namespace Aspire.Hosting.ApplicationModel;

public class MessageQueueDashboardResource(string name)
    : ContainerResource(name)
{
    internal const string HttpsEndpointName = "https";
    internal const string HttpEndpointName = "http";

    private EndpointReference? _httpReference;
    private EndpointReference? _httpsReference;

    public EndpointReference HttpEndpoint =>
        _httpReference ??= new(this, HttpEndpointName);

    public EndpointReference HttpsEndpoint =>
        _httpsReference ??= new(this, HttpsEndpointName);
}
