using HTTPServer.Extensions;

using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace HTTPServer.Services.Models;

public class HttpRequest
{
    public TcpClient? Client { get; set; }

    public bool IsSslEnabled { get; set; }
    public SslStream? SslStream { get; set; }

    public required HttpMethod Method { get; set; }
    public required string Path { get; set; }
    public required string ProtocolVersion { get; set; }

    public required Dictionary<string, string> Headers { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach(var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        return sb.ToString();
    }

    public async Task RespondAsync(HttpResponse response)
    {
        var sb = new StringBuilder();

        sb.Append($"HTTP/1.1 {(int)response.StatusCode} {response.StatusCode.GetFriendlyName()}\r\n");

        foreach (var header in response.Headers)
        {
            sb.Append($"{header.Key}: {header.Value}\r\n");
        }

        sb.Append("\r\n");

        if (response.Content is not null)
        {
            sb.Append($"{response.Content}\r\n");
        }

        byte[] data = Encoding.UTF8.GetBytes(sb.ToString());

        if(IsSslEnabled)
        {
            if(SslStream is null)
            {
                throw new Exception("HttpRequest is flagged as having Ssl enabled but the SslStream was null during response.");
            }

            await SslStream.WriteAsync(data);
        }
        else
        {
            if (Client is null)
            {
                throw new Exception("Client was null during response.");
            }

            await Client.GetStream().WriteAsync(data);
        }
    }
}
