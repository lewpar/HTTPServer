# HTTPServer
This is a basic HTTP server that I am writing to learn more about the HTTP protocol.

It has basic support for these MIME types:
- .html (text/html)
- .css (text/css)
- .txt (text/plain)

With support for other file types coming later when I decide to add them.

## SSL
The HTTP server is currently built to only support SSL. The HTTP server will redirect any connection attempts to the HTTPS server.