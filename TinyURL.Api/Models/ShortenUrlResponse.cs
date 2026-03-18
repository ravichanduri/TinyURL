namespace TinyURL.Api.Models;

public sealed record ShortenUrlResponse(
    string ShortCode,
    string ShortUrl
);

