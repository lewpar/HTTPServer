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
            _ = HandleHttpConnectionAsync(new HttpRequest()
            { 
                Client = client,
                Headers = await client.GetStream().ReadHttpHeadersAsync()
            });
        }
    }

    private async Task HandleHttpConnectionAsync(HttpRequest request)
    {
        try
        {
            logger.LogInformation($"Client '{request.Client.Client.RemoteEndPoint}' connected to HTTP server, redirecting to HTTPS..");

            await request.RespondAsync(new HttpResponse(HttpStatus.MovedPermanently)
            {
                Content = "The HTTP version of this site is disabled, redirecting to HTTPS secured website.",
                Headers = new Dictionary<string, string>()
                {
                    { "Location", "https://localhost/" }
                }
            });
        }
        catch(Exception ex)
        {
            logger.LogCritical($"{ex.Message} {ex.StackTrace}");
        }
    }
}