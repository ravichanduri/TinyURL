namespace TinyURL.Api.Services;

using TinyURL.Api.Models;

public interface IUrlShortenerService
{
    UrlMapping Create(string originalUrl, string baseUrl);
    bool TryGet(string shortCode, out UrlMapping mapping);
    IReadOnlyCollection<UrlMapping> GetAll();
}

