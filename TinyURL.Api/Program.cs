var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<TinyURL.Api.Services.IUrlShortenerService, TinyURL.Api.Services.InMemoryUrlShortenerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapControllers();

// OpenAPI JSON (hand-written, no NuGet dependency)
app.MapGet("/swagger/v1/swagger.json", (HttpContext ctx) =>
{
    var serverUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";

    var doc = new
    {
        openapi = "3.0.1",
        info = new { title = "TinyURL API", version = "v1" },
        servers = new[] { new { url = serverUrl } },
        paths = new Dictionary<string, object>
        {
            ["/shorten"] = new
            {
                post = new
                {
                    summary = "Create a short URL",
                    requestBody = new
                    {
                        required = true,
                        content = new Dictionary<string, object>
                        {
                            ["application/json"] = new
                            {
                                schema = new { @ref = "#/components/schemas/ShortenUrlRequest" }
                            }
                        }
                    },
                    responses = new Dictionary<string, object>
                    {
                        ["200"] = new
                        {
                            description = "OK",
                            content = new Dictionary<string, object>
                            {
                                ["application/json"] = new
                                {
                                    schema = new { @ref = "#/components/schemas/ShortenUrlResponse" }
                                }
                            }
                        },
                        ["400"] = new { description = "Bad Request" }
                    }
                }
            },
            ["/{shortCode}"] = new
            {
                get = new
                {
                    summary = "Redirect to original URL",
                    parameters = new object[]
                    {
                        new
                        {
                            name = "shortCode",
                            @in = "path",
                            required = true,
                            schema = new { type = "string" }
                        }
                    },
                    responses = new Dictionary<string, object>
                    {
                        ["302"] = new { description = "Found" },
                        ["404"] = new { description = "Not Found" }
                    }
                }
            },
            ["/urls"] = new
            {
                get = new
                {
                    summary = "List all URL mappings",
                    responses = new Dictionary<string, object>
                    {
                        ["200"] = new
                        {
                            description = "OK",
                            content = new Dictionary<string, object>
                            {
                                ["application/json"] = new
                                {
                                    schema = new
                                    {
                                        type = "array",
                                        items = new { @ref = "#/components/schemas/UrlMappingDto" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        },
        components = new
        {
            schemas = new Dictionary<string, object>
            {
                ["ShortenUrlRequest"] = new
                {
                    type = "object",
                    required = new[] { "url" },
                    properties = new Dictionary<string, object>
                    {
                        ["url"] = new { type = "string", example = "https://example.com/some/long/path" }
                    }
                },
                ["ShortenUrlResponse"] = new
                {
                    type = "object",
                    required = new[] { "shortCode", "shortUrl" },
                    properties = new Dictionary<string, object>
                    {
                        ["shortCode"] = new { type = "string", example = "aB3kZ9" },
                        ["shortUrl"] = new { type = "string", example = $"{serverUrl}/aB3kZ9" }
                    }
                },
                ["UrlMappingDto"] = new
                {
                    type = "object",
                    required = new[] { "shortCode", "originalUrl", "createdAt", "shortUrl" },
                    properties = new Dictionary<string, object>
                    {
                        ["shortCode"] = new { type = "string" },
                        ["originalUrl"] = new { type = "string" },
                        ["createdAt"] = new { type = "string", format = "date-time" },
                        ["shortUrl"] = new { type = "string" }
                    }
                }
            }
        }
    };

    return Results.Json(doc);
});

// Optional: keep the previous OpenAPI URL shape
app.MapGet("/openapi/v1.json", () => Results.Redirect("/swagger/v1/swagger.json"));

// Minimal Swagger UI without extra NuGet packages (loads Swagger UI from CDN)
app.MapGet("/swagger", (HttpContext ctx) =>
{
    const string html = """
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>TinyURL API - Swagger UI</title>
    <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5/swagger-ui.css" />
    <style>
      html, body { height: 100%; margin: 0; }
    </style>
  </head>
  <body>
    <div id="swagger-ui"></div>
    <script src="https://unpkg.com/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
    <script>
      window.ui = SwaggerUIBundle({
        url: "/swagger/v1/swagger.json",
        dom_id: "#swagger-ui",
        deepLinking: true,
        persistAuthorization: true
      });
    </script>
  </body>
</html>
""";

    ctx.Response.ContentType = "text/html; charset=utf-8";
    return ctx.Response.WriteAsync(html);
});

app.Run();
