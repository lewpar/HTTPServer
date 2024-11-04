using System.Net.Security;
using System.Text;

namespace HTTPServer.Extensions;

public static class StreamExtensions
{
    public static async Task<Dictionary<string, string>> ReadHttpHeadersAsync(this Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var headers = new Dictionary<string, string>();

        string? line;

        while ((line = await reader.ReadLineAsync()) is not null)
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
