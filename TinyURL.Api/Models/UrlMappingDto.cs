namespace TinyURL.Api.Models;

public sealed record UrlMappingDto(
    string ShortCode,
    string OriginalUrl,
    DateTimeOffset CreatedAt,
    string ShortUrl
);

