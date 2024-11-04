using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

using HTTPServer.Services.Models;
using HTTPServer.Extensions;

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
        try
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

            var request = await ReadHttpRequestAsync(client, sslStream);
            await HandleHttpRequestAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogCritical($"{ex.Message}: {ex.StackTrace}");
        }
    }

    private async Task<HttpRequest> ReadHttpRequestAsync(TcpClient client, SslStream sslStream)
    {
        return new HttpRequest()
        { 
            Client = client,
            IsSslEnabled = true,
            SslStream = sslStream,
            Headers = await sslStream.ReadHttpHeadersAsync()
        };
    }

    private async Task HandleHttpRequestAsync(HttpRequest request)
    {
        logger.LogInformation($"Received request: {Environment.NewLine}{request.ToString()}");

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

        logger.LogInformation("Sending response..");

        await request.RespondAsync(new HttpResponse(HttpStatus.OK)
        {
            Content = markup,
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "text/html; charset=UTF-8" },
                { "Date", $"{DateTime.Now.ToString("DDD, dd MMM yyyy hh:mm:ss GMT")}" },
                { "Content-Length", $"{markup.Length}" }
            }
        });
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
