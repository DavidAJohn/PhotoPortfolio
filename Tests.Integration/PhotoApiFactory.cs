using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhotoPortfolio.Server;
using PhotoPortfolio.Server.Data;
using Testcontainers.MongoDb;

namespace PhotoPortfolio.Tests.Integration;

public class PhotoApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    public const string TestDbName = "PhotoPortfolioTestDb";
    public const string SitePreferencesId = "63ff656c79461a7346026485";

    private readonly MongoDbContainer _dbContainer =
        new MongoDbBuilder()
            .WithImage("mongo:5")
            .WithEnvironment("MONGO_INITDB_DATABASE", TestDbName)
            .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(MongoContext));
            services.AddSingleton(_ =>
                    new MongoContext(_dbContainer.GetConnectionString(), TestDbName));
        });

        builder.UseConfiguration(new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "SitePreferencesId", SitePreferencesId }
                    })
                    .Build());
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
