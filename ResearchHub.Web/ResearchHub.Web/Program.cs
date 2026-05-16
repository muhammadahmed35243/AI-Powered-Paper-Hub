using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ResearchHub.Shared;
using ResearchHub.Web;
using ResearchHub.Web.Services;
using System.Net.Http.Headers;

EnvLoader.Load();
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var settings = new AppSettings
{
    ApiBaseUrl = builder.Configuration["ApiBaseUrl"]
        ?? Environment.GetEnvironmentVariable("ApiBaseUrl")
        ?? "http://localhost:5291",
    GoogleClientId = builder.Configuration["Google:ClientId"]
        ?? Environment.GetEnvironmentVariable("Google__ClientId")
        ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientId")
        ?? ""
};
builder.Services.AddSingleton(settings);

builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiClient>();

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/") };
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
});

await builder.Build().RunAsync();
