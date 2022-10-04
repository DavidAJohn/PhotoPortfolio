using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PhotoPortfolio.Client;
using PhotoPortfolio.Client.Contracts;
using PhotoPortfolio.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("PhotoPortfolio.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api/"));

builder.Services.AddHttpClient("PhotoPortfolio.ServerAPI.Secure", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api/"))
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
