using HTTPServer.Services.Models;

namespace HTTPServer.Extensions;

public static class EnumExtensions
{
    public static string GetHttpProtocolName(this HttpStatus status)
    {
        switch(status)
        {
            case HttpStatus.OK:
                return "OK";

            case HttpStatus.MovedPermanently:
                return "Moved Permanently";

            case HttpStatus.FileNotFound:
                return "Not Found";

            case HttpStatus.InternalError:
                return "Internal Server Error";
        }

        throw new NotImplementedException($"Missing friendly name for {status} HttpStatus.");
    }
}
