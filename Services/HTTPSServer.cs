﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using HTTPServer.Services.Models;
using HTTPServer.Extensions;
using HTTPServer.Configuration;
using System.Text;

namespace HTTPServer.Services;

internal class HTTPSServer : BackgroundService
{
    private TcpListener _httpsListener;

    private readonly ILogger<HTTPSServer> logger;
    private readonly HTTPServerConfig config;
    private readonly IFetchContent fetchService;

    public HTTPSServer(ILogger<HTTPSServer> logger, HTTPServerConfig config,
        IFetchContent fetchService)
    {
        _httpsListener = new TcpListener(IPAddress.Any, 443);

        this.logger = logger;
        this.config = config;
        this.fetchService = fetchService;
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

            client.Close();
        }
        catch (Exception ex)
        {
            logger.LogCritical($"{ex.Message}: {ex.StackTrace}");
        }
    }

    private async Task<HttpRequest> ReadHttpRequestAsync(TcpClient client, SslStream sslStream)
    {
        var request = await sslStream.ReadHttpRequest();
        request.Client = client;
        request.IsSslEnabled = true;
        request.SslStream = sslStream;

        return request;
    }

    private async Task HandleHttpRequestAsync(HttpRequest request)
    {
        logger.LogInformation($"Received request: {Environment.NewLine}{request.ToString()}");

        byte[]? data;
        try
        {
            data = await fetchService.GetContentAsync(request.RequestPath);
        }
        catch(Exception ex)
        {
            var content = "File not found.";
            var fileNotFoundResponse = new HttpResponse(HttpStatus.FileNotFound)
            {
                Content = content,
                Headers = new Dictionary<string, string>()
                {
                    { "Content-Type", "text/html; charset=UTF-8" },
                    { "Content-Length", content.Length.ToString() }
                }
            };

            await request.RespondAsync(fileNotFoundResponse);

            logger.LogCritical($"{ex.Message} {ex.StackTrace}");
            return;
        }

        if(data is null)
        {
            var content = "An internal error occured.";
            var internalErrorResponse = new HttpResponse(HttpStatus.InternalError)
            {
                Content = content,
                Headers = new Dictionary<string, string>()
                {
                    { "Content-Type", "text/html; charset=UTF-8" },
                    { "Content-Length", content.Length.ToString() }
                }
            };

            await request.RespondAsync(internalErrorResponse);

            logger.LogCritical($"An internal error occured for request: {request.ToString()}");
            return;
        }

        var response = new HttpResponse(HttpStatus.OK)
        {
            Content = Encoding.UTF8.GetString(data),
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", request.GetMIMEType(request.RequestPath.GetHttpRequestFileName()) },
                { "Date", $"{DateTime.Now.ToString("ddd dd MMM yyyy hh:mm:ss GMT")}" },
                { "Content-Length", $"{data.Length}" }
            }
        };

        logger.LogInformation($"Sent response: {Environment.NewLine}{response.ToString()}");

        await request.RespondAsync(response);
    }

    private X509Certificate2? GetServerCertificate()
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        store.Open(OpenFlags.ReadOnly);

        var cert = store.Certificates.FirstOrDefault(c => c.Thumbprint.ToLower() == config.CertificateThumbprint);

        return cert;
    }
}
