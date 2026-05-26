using System.Net.Http.Headers;

namespace PussyCats.App.Configuration;

public sealed class JwtForwardingHandler : DelegatingHandler
{
    private readonly SessionContext session;

    public JwtForwardingHandler(SessionContext session)
    {
        this.session = session;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(session.JwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
