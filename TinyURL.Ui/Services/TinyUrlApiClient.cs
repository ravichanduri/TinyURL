using System.Net;
using System.Net.Http.Json;
using TinyURL.Ui.Models;

namespace TinyURL.Ui.Services;

public sealed class TinyUrlApiClient
{
    private readonly HttpClient _http;

    public TinyUrlApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ShortenUrlResponse> ShortenAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("shorten", new ShortenUrlRequest(url), cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<ShortenUrlResponse>(cancellationToken: cancellationToken);
            return payload ?? throw new InvalidOperationException("API returned an empty response.");
        }

        var error = await TryReadApiErrorAsync(response, cancellationToken);
        throw new TinyUrlApiException(response.StatusCode, error);
    }

    public async Task<IReadOnlyList<UrlMappingDto>> GetUrlsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("urls", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadFromJsonAsync<List<UrlMappingDto>>(cancellationToken: cancellationToken);
            return payload ?? [];
        }

        var error = await TryReadApiErrorAsync(response, cancellationToken);
        throw new TinyUrlApiException(response.StatusCode, error);
    }

    private static async Task<string?> TryReadApiErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiError>(cancellationToken: cancellationToken);
            if (!string.IsNullOrWhiteSpace(problem?.Error))
                return problem.Error;
        }
        catch
        {
        }

        try
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(raw) ? null : raw;
        }
        catch
        {
            return null;
        }
    }
}

public sealed class TinyUrlApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public TinyUrlApiException(HttpStatusCode statusCode, string? message)
        : base(string.IsNullOrWhiteSpace(message) ? $"API error: {(int)statusCode} {statusCode}" : message)
    {
        StatusCode = statusCode;
    }
}

