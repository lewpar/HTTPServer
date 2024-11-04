using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Net.Sockets;
using System.Text;

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
            _ = HandleHttpConnectionAsync(client);
        }
    }

    private async Task HandleHttpConnectionAsync(TcpClient client)
    {
        logger.LogInformation($"Client '{client.Client.RemoteEndPoint}' connected to HTTP server, redirecting to HTTPS..");

        var redirectResponse = 
            "HTTP/1.1 301 Moved Permanently\r\n" +
            "Location: https://localhost/\r\n" +
            "\r\n";

        byte[] response = Encoding.UTF8.GetBytes(redirectResponse);
        await client.GetStream().WriteAsync(response);
    }
}