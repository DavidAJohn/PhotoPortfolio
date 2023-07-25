namespace PhotoPortfolio.Server.Helpers;

public static class HttpContextExtensions
{
    public static string GetAppBaseUrl(
            this HttpContext httpContext
        )
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var currentContext = httpContext;
        var baseUrl = $"{currentContext.Request.Scheme}://{currentContext.Request.Host}";

        return baseUrl;
    }
}
