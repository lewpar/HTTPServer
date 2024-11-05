using HTTPServer.Configuration;
using HTTPServer.Extensions;

namespace HTTPServer.Services;

public class FileSystemFetchContent : IFetchContent
{
    private readonly HTTPServerConfig config;

    public FileSystemFetchContent(HTTPServerConfig config)
    {
        this.config = config;
    }

    public async Task<byte[]> GetContentAsync(string path)
    {
        var fileName = path.GetHttpRequestFileName();

        // Remove the leading slash to allow Path.Combine to properly combine.
        fileName = fileName.TrimStart('/');

        var rootPath = Path.GetFullPath(config.ContentPath);
        var requestedPath = Path.GetFullPath(Path.Combine(rootPath, fileName));

        if(!requestedPath.StartsWith(rootPath))
        {
            throw new Exception("Tried to access path outside of root path.");
        }

        if(!File.Exists(requestedPath))
        {
            throw new Exception("File not found.");
        }

        return await File.ReadAllBytesAsync(requestedPath);
    }
}
