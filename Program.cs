using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace HTTPServer;

class Program
{
    static async Task Main(string[] args)
    {
        var app = Host.CreateApplicationBuilder(args);

        app.Services.AddHostedService<Services.HTTPServer>();
        app.Services.AddHostedService<Services.HTTPSServer>();

        var host = app.Build();

        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }
}