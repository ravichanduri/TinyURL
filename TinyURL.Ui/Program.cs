using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TinyURL.Ui;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        baseUrl = "http://localhost:5244/";

    if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
        baseUrl += "/";

    return new HttpClient { BaseAddress = new Uri(baseUrl, UriKind.Absolute) };
});

builder.Services.AddScoped<TinyURL.Ui.Services.TinyUrlApiClient>();

await builder.Build().RunAsync();
