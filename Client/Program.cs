using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PhotoPortfolio.Client;
using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Client.Services;
using Polly.Extensions.Http;
using Polly;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("PhotoPortfolio.ServerAPI", client => client.BaseAddress = 
    new Uri(builder.HostEnvironment.BaseAddress + "api/"))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("PhotoPortfolio.ServerAPI.Secure", client => client.BaseAddress = 
    new Uri(builder.HostEnvironment.BaseAddress + "api/"))
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("PhotoPortfolio.ServerAPI.Secure"));

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://6a199035-51cf-46f8-b027-9e50080aa135/API.Access");
    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddBlazoredModal();

builder.Services.AddMudServices();

await builder.Build().RunAsync();

IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    // Retry with jitter: https://github.com/App-vNext/Polly/wiki/Retry-with-jitter
    Random jitter = new Random();

    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 5,
            sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))  // exponential backoff (2, 4, 8, 16, 32 secs)
                  + TimeSpan.FromMilliseconds(jitter.Next(0, 1000))  // plus some jitter: up to 1 second
            );
}

IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)
        );
}
