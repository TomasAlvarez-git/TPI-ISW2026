using System.Net;

namespace Dsw2026Tpi.E2ETests.Helpers;

public class RateLimitRetryHandler : DelegatingHandler
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(13);

    public RateLimitRetryHandler() : base(new HttpClientHandler()) { }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            var response = await base.SendAsync(request, cancellationToken);

            var rateLimited = response.StatusCode is HttpStatusCode.ServiceUnavailable
                                                  or HttpStatusCode.TooManyRequests;
            if (!rateLimited || attempt >= MaxRetries)
                return response;

            response.Dispose();
            await Task.Delay(WaitTime, cancellationToken);
        }
    }
}