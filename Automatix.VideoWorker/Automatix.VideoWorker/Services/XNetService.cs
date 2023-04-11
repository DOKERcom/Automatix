using Leaf.xNet;

namespace Automatix.VideoWorker.Services;

public class XNetService
{
    private readonly HttpRequest _httpRequest;

    public XNetService()
    {
        _httpRequest = new HttpRequest();
    }

    public HttpResponse Get(string href)
    {
        var response = _httpRequest.Get(href);

        return response;
    }

    public HttpResponse Post(string href, string str = "", string contentType = "application/x-www-form-urlencoded")
    {
        var response = _httpRequest.Post(href, str, contentType);

        return response;
    }

    public MemoryStream DownloadFile(string href)
    {
        var response = _httpRequest.Get(href).ToMemoryStream();

        return response;
    }
}