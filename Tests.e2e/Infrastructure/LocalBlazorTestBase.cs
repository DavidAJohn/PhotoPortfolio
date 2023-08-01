using Microsoft.AspNetCore.Mvc.Testing;

namespace PhotoPortfolio.Tests.e2e;

public class LocalBlazorTestBase : PageTest
{
    // This code was adapted from:
    // https://github.com/MackinnonBuck/blazor-playwright-example/tree/main/BlazorWasmPlaywright

    // It was demonstrated by Mackinnon Buck in this video:
    // ASP.NET Community Standup - Blazor App Testing with Playwright
    // https://www.youtube.com/live/lJa3YlUliEs

    protected static readonly Uri RootUri = new("http://127.0.0.1");

    private readonly WebApplicationFactory<Program> _webApplicationFactory = new();

    private HttpClient? _httpClient;

    [SetUp]
    public async Task LocalBlazorTestBaseSetup()
    {
        _httpClient = _webApplicationFactory.CreateClient(new()
        {
            BaseAddress = RootUri,
        });

        await Context.RouteAsync($"{RootUri.AbsoluteUri}**", async route =>
        {
            var request = route.Request;
            var content = request.PostDataBuffer is { } postDataBuffer
                ? new ByteArrayContent(postDataBuffer)
                : null;

            var requestMessage = new HttpRequestMessage(new(request.Method), request.Url)
            {
                Content = content,
            };

            foreach (var header in request.Headers)
            {
                requestMessage.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var responseHeaders = response.Content.Headers.Select(h => KeyValuePair.Create(h.Key, string.Join(",", h.Value)));

            await route.FulfillAsync(new()
            {
                BodyBytes = responseBody,
                Headers = responseHeaders,
                Status = (int)response.StatusCode,
            });
        });
    }

    [TearDown]
    public void LocalBlazorTestBaseTearDown()
    {
        _httpClient?.Dispose();
    }
}
