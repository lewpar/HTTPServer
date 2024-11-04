using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HTTPServer.Configuration;

namespace HTTPServer;

class Program
{
    static async Task Main(string[] args)
    {
        var app = Host.CreateApplicationBuilder(args);

        app.Services.AddHostedService<Services.HTTPServer>();
        app.Services.AddHostedService<Services.HTTPSServer>();

        app.Services.AddScoped<Services.IFetchContent, Services.FileSystemFetchContent>();

        var config = app.Configuration.GetSection("HTTPServer").Get<HTTPServerConfig>();
        if(config is null)
        {
            throw new Exception("Failed to load HTTP server config.");
        }

        app.Services.AddSingleton<HTTPServerConfig>(config);

        var host = app.Build();

        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }
}