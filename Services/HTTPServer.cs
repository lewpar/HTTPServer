using HTTPServer.Extensions;
using HTTPServer.Services.Models;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Net.Sockets;

namespace HTTPServer.Services;

public class HTTPServer : BackgroundService
{
    private TcpListener _httpListener;

    private readonly ILogger<HTTPServer> logger;

    public HTTPServer(ILogger<HTTPServer> logger)
    {
        _httpListener = new TcpListener(IPAddress.Any, 80);

        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting HTTP server..");

        _httpListener.Start();
        logger.LogInformation("HTTP server started.");

        logger.LogInformation("Listening for HTTP connections..");
        await AcceptHttpClientsAsync(stoppingToken);
    }

    private async Task AcceptHttpClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _httpListener.AcceptTcpClientAsync();

            var request = await client.GetStream().ReadHttpRequest();
            request.Client = client;

            _ = HandleHttpConnectionAsync(request);
        }
    }

    private async Task HandleHttpConnectionAsync(HttpRequest request)
    {
        try
        {
            logger.LogInformation($"Client '{request.Client?.Client.RemoteEndPoint}' connected to HTTP server, redirecting to HTTPS..");

            var host = request.Headers.FirstOrDefault(h => h.Key.ToLower() == "host");
            if(string.IsNullOrWhiteSpace(host.Key))
            {
                logger.LogCritical($"No host header was found for HTTP request: {request.ToString()}");
                return;
            }

            var redirectResponse = new HttpResponse(HttpStatus.MovedPermanently)
            {
                Headers = new Dictionary<string, string>()
                {
                    { "Location", $"https://{host.Value}{request.RequestPath}" }
                }
            };

            logger.LogInformation($"Sending redirect response: {Environment.NewLine} {redirectResponse.ToString()}");

            await request.RespondAsync(redirectResponse);
        }
        catch(Exception ex)
        {
            logger.LogCritical($"{ex.Message} {ex.StackTrace}");
        }
        finally
        {
            request.Client?.Close();
        }
    }
}