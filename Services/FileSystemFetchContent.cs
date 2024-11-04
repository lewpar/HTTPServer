using HTTPServer.Configuration;

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
        // Special case for root path.
        if(path == "/")
        {
            path = "/index.html";
        }

        // Remove the leading slash to allow Path.Combine to properly combine.
        path = path.TrimStart('/');

        var rootPath = Path.GetFullPath(config.ContentPath);
        var requestedPath = Path.GetFullPath(Path.Combine(rootPath, path));

        if(!requestedPath.StartsWith(rootPath))
        {
            throw new Exception("Tried to access path outside of root path.");
        }

        return await File.ReadAllBytesAsync(requestedPath);
    }
}
