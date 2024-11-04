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
}
