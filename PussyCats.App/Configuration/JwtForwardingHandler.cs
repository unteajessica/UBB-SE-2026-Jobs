using System.Net;
using System.Net.Http.Headers;

namespace PussyCats.App.Configuration;

public sealed class JwtForwardingHandler : DelegatingHandler
{
    private readonly SessionContext session;

    public JwtForwardingHandler(SessionContext session)
    {
        this.session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(session.JwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.JwtToken);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            session.SignOut();
            await UIDispatcher.EnqueueAsync(() =>
            {
                if (PussyCats_App.App.MainAppWindow is PussyCats_App.MainWindow mainWindow)
                    mainWindow.ShowLogin();
            });
        }

        return response;
    }
}
