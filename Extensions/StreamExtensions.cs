using HTTPServer.Exceptions;
using HTTPServer.Services.Models;

using System.Text;
using System.Text.RegularExpressions;

namespace HTTPServer.Extensions;

public static class StreamExtensions
{
    public static async Task<HttpRequest> ReadHttpRequest(this Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        var requestLine = await reader.ReadLineAsync();
        if(requestLine is null)
        {
            throw new MalformedHttpRequestException();
        }

        var match = Regex.Match(requestLine, "([A-Z]+?) (.+?) (.+?)\\/([0-9]+\\.?[0-9]?)");
        if(!match.Success || match.Groups.Count < 5) 
        {
            throw new MalformedHttpRequestException();
        }

        var method = match.Groups[1].Value.GetHttpMethod();
        if(method is null)
        {
            throw new MalformedHttpRequestException();
        }

        var path = match.Groups[2].Value;

        var protocol = match.Groups[3].Value;
        var protocolVersion = match.Groups[4].Value;

        var headers = await stream.ReadHttpHeadersAsync(reader);

        var request = new HttpRequest()
        {
            Method = method.Value,
            RequestPath = path,
            ProtocolVersion = $"{protocol}/{protocolVersion}",
            Headers = headers
        };

        return request;
    }

    public static async Task<Dictionary<string, string>> ReadHttpHeadersAsync(this Stream stream, StreamReader? reader = null)
    {
        using var sr = reader ?? new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var headers = new Dictionary<string, string>();

        string? line;

        while ((line = await sr.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            var parts = line.Split(":");

            if (parts.Length < 2)
            {
                continue;
            }

            headers.Add(parts[0], parts[1]);
        }

        return headers;
    }
}
