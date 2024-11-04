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
}
