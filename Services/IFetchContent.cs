namespace HTTPServer.Services;

public interface IFetchContent
{
    Task<byte[]> GetContentAsync(string path);
}
