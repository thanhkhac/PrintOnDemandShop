namespace CleanArchitectureBase.Infrastructure.Loggers;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class RefitLoggingHandler : DelegatingHandler
{
    private readonly ILogger<RefitLoggingHandler> _logger;

    public RefitLoggingHandler(ILogger<RefitLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("➡️ Refit Request: {method} {url}", request.Method, request.RequestUri);
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Request Body:\n{body}", requestBody);
        }

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("⬅️ Response: {statusCode}", response.StatusCode);
        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response Body:\n{body}", responseBody);
        }

        return response;
    }
}
