namespace HTTPServer.Configuration;

public class HTTPServerConfig
{
    public required string ContentPath { get; set; }
    public required string CertificateThumbprint { get; set; }
}
