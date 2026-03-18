namespace TinyURL.Api.Models;

public sealed record UrlMapping(
    string ShortCode,
    string OriginalUrl,
    DateTimeOffset CreatedAt,
    string ShortUrl
);

