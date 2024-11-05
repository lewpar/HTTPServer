using System.Text;

namespace HTTPServer.Services.Models;

public class HttpResponse
{
    public HttpStatus StatusCode { get; }
    public string? Content { get; set; }
    public Dictionary<string, string> Headers { get; set; }

    public HttpResponse(HttpStatus statusCode)
    {
        StatusCode = statusCode;
        Headers = new Dictionary<string, string>();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"HTTP/1.1 {(int)StatusCode} {StatusCode}");

        foreach (var header in Headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        return sb.ToString();
    }
}
