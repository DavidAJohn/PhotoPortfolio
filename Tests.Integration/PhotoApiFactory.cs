﻿using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
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

    private readonly IContainer _stripeContainer = 
        new ContainerBuilder()
            .WithImage("stripe/stripe-mock:latest")
            .WithPortBinding(12111, true)
            .Build();

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
                        { "SitePreferencesId", SitePreferencesId },
                        { "Stripe:SecretKey", "sk_test_123" },
                        { "Stripe:ApiBase", $"http://localhost:{_stripeContainer.GetMappedPublicPort(12111)}" }
                    })
                    .Build());
    }

    public async Task InitializeAsync()
    {
        await _stripeContainer.StartAsync();
        await _dbContainer.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _stripeContainer.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }
}
