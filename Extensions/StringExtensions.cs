namespace HTTPServer.Extensions;

public static class StringExtensions
{
    public static Services.Models.HttpMethod? GetHttpMethod(this string method)
    {
        switch(method)
        {
            case "GET":
                return Services.Models.HttpMethod.GET;
        }

        return null;
    }

    public static string GetHttpRequestFileName(this string path)
    {
        string result = path;
        result = result.Trim();

        if(path == "/")
        {
            result = "/index.html";
        }

        return result;
    }
}
