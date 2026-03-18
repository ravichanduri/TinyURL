namespace TinyURL.Ui.Models;

public sealed record ShortenUrlRequest(string Url);

public sealed record ShortenUrlResponse(string ShortCode, string ShortUrl);

public sealed record UrlMappingDto(string ShortCode, string OriginalUrl, DateTimeOffset CreatedAt, string ShortUrl);

public sealed record ApiError(string? Error);

