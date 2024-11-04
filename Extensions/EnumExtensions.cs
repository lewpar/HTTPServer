using HTTPServer.Services.Models;

namespace HTTPServer.Extensions;

public static class EnumExtensions
{
    public static string GetFriendlyName(this HttpStatus status)
    {
        switch(status)
        {
            case HttpStatus.OK:
                return "OK";

            case HttpStatus.MovedPermanently:
                return "Moved Permanently";
        }

        throw new NotImplementedException($"Missing friendly name for {status} HttpStatus.");
    }
}
