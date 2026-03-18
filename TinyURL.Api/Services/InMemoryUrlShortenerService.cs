namespace TinyURL.Api.Services;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using TinyURL.Api.Models;

public sealed class InMemoryUrlShortenerService : IUrlShortenerService
{
    private const int ShortCodeLength = 6; // 5–8 required; choose a stable default

    private readonly ConcurrentDictionary<string, UrlMapping> _byCode = new(StringComparer.OrdinalIgnoreCase);

    public UrlMapping Create(string originalUrl, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(originalUrl))
            throw new ArgumentException("URL is required.", nameof(originalUrl));

        while (true)
        {
            var code = GenerateCode(ShortCodeLength);
            var mapping = new UrlMapping(
                ShortCode: code,
                OriginalUrl: originalUrl,
                CreatedAt: DateTimeOffset.UtcNow,
                ShortUrl: CombineBaseAndCode(baseUrl, code)
            );

            if (_byCode.TryAdd(code, mapping))
                return mapping;
        }
    }

    public bool TryGet(string shortCode, out UrlMapping mapping) =>
        _byCode.TryGetValue(shortCode, out mapping!);

    public IReadOnlyCollection<UrlMapping> GetAll() => _byCode.Values.ToArray();

    private static string CombineBaseAndCode(string baseUrl, string shortCode)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            return shortCode;

        baseUrl = baseUrl.TrimEnd('/');
        return $"{baseUrl}/{shortCode}";
    }

    private static string GenerateCode(int length)
    {
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);

        Span<char> chars = stackalloc char[length];
        for (var i = 0; i < length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];

        return new string(chars);
    }
}

