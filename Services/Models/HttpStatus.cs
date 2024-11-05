namespace HTTPServer.Services.Models;

public enum HttpStatus
{
    OK = 200,
    MovedPermanently = 301,
    FileNotFound = 404,
    InternalError = 500
}
