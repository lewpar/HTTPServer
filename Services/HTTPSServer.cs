using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace HTTPServer.Services;

internal class HTTPSServer : BackgroundService
{
    private TcpListener _httpsListener;

    private readonly ILogger<HTTPSServer> logger;

    public HTTPSServer(ILogger<HTTPSServer> logger)
    {
        _httpsListener = new TcpListener(IPAddress.Any, 443);

        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting HTTPS server..");

        _httpsListener.Start();
        logger.LogInformation("HTTPS server started.");

        logger.LogInformation("Listening for HTTPS connections..");
        await AcceptHttpsClientsAsync(stoppingToken);
    }

    private async Task AcceptHttpsClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _httpsListener.AcceptTcpClientAsync();
            _ = HandleHttpsConnectionAsync(client);
        }
    }

    private async Task HandleHttpsConnectionAsync(TcpClient client)
    {
        logger.LogInformation($"Client '{client.Client.RemoteEndPoint}' connected.");

        var stream = client.GetStream();
        var sslStream = new SslStream(stream, true);

        var serverCertificate = GetServerCertificate();
        if (serverCertificate is null)
        {
            logger.LogError("Failed to get server certificate.");
            return;
        }

        await sslStream.AuthenticateAsServerAsync(serverCertificate);

        string markup = """
            <!DOCTYPE html5>
            <html>
            <head>
            <title>HTTPServer</title>
            </head>
            <body>
            <h1>Hello, World!</h1>
            </body>
            </html>
            """;

        string okResponse =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/html; charset=UTF-8\r\n" +
            $"Date: {DateTime.Now.ToString("DDD, dd MMM yyyy hh:mm:ss GMT")}\r\n" +
            $"Content-Length: {markup.Length}\r\n" +
            "\r\n" +
            $"{markup}";

        byte[] response = Encoding.UTF8.GetBytes(okResponse);
        await sslStream.WriteAsync(response);
    }

    private X509Certificate2? GetServerCertificate()
    {
        var thumbprint = "fb3a3eeed998c4bd7620859b738bc67bf514dd9b";
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        store.Open(OpenFlags.ReadOnly);

        var cert = store.Certificates.FirstOrDefault(c => c.Thumbprint.ToLower() == thumbprint);

        return cert;
    }
}
