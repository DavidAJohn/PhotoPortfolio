namespace PhotoPortfolio.Server.Helpers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.File;

public static class FileConfigurationExtensions
{
    public static IServiceCollection ConfigureOcelotPlaceholders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.PostConfigure<FileConfiguration>(fileConfiguration =>
        {
            var prodigiApi = configuration.GetSection("Prodigi");

            foreach (var route in fileConfiguration.Routes)
            {
                ConfigureRoute(route, prodigiApi);
            }
        });

        return services;
    }

    private static void ConfigureRoute(FileRoute route, IConfigurationSection prodigiApi)
    {
        var apiKey = prodigiApi.GetValue<string>("ApiKey");

        if (route.UpstreamHeaderTransform.ContainsKey("X-API-Key"))
        {
            route.UpstreamHeaderTransform.Remove("X-API-Key");
            route.UpstreamHeaderTransform.Add("X-API-Key", apiKey);
        }
        else
        {
            route.UpstreamHeaderTransform.Add("X-API-Key", apiKey);
        }
    }
}
