namespace TinyURL.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using TinyURL.Api.Models;
using TinyURL.Api.Services;

[ApiController]
public sealed class UrlsController : ControllerBase
{
    private readonly IUrlShortenerService _service;

    public UrlsController(IUrlShortenerService service)
    {
        _service = service;
    }

    /// <summary>Create a short URL for a given long URL.</summary>
    [HttpPost("/shorten")]
    [ProducesResponseType(typeof(ShortenUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ShortenUrlResponse> Shorten([FromBody] ShortenUrlRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(new { error = "URL is required." });

        if (!TryValidateHttpUrl(request.Url, out var normalized))
            return BadRequest(new { error = "URL must be a valid http:// or https:// URL." });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var mapping = _service.Create(normalized, baseUrl);

        return Ok(new ShortenUrlResponse(mapping.ShortCode, mapping.ShortUrl));
    }

    /// <summary>Redirect to the original URL by short code.</summary>
    [HttpGet("/{shortCode}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RedirectToOriginal([FromRoute] string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            return NotFound();

        if (!_service.TryGet(shortCode, out var mapping))
            return NotFound(new { error = "Short code not found." });

        return Redirect(mapping.OriginalUrl);
    }

    /// <summary>List all stored short URL mappings.</summary>
    [HttpGet("/urls")]
    [ProducesResponseType(typeof(IEnumerable<UrlMappingDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<UrlMappingDto>> List()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var results = _service
            .GetAll()
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new UrlMappingDto(
                ShortCode: m.ShortCode,
                OriginalUrl: m.OriginalUrl,
                CreatedAt: m.CreatedAt,
                ShortUrl: string.IsNullOrWhiteSpace(m.ShortUrl) ? $"{baseUrl}/{m.ShortCode}" : m.ShortUrl
            ))
            .ToArray();

        return Ok(results);
    }

    private static bool TryValidateHttpUrl(string input, out string normalized)
    {
        normalized = string.Empty;

        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri))
            return false;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;

        normalized = uri.ToString();
        return true;
    }
}

