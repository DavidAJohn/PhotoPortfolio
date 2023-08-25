using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using PhotoPortfolio.Server.Services;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
        //.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .WriteTo.Console()
        .WriteTo.File("logs/errors.txt", restrictedToMinimumLevel: LogEventLevel.Warning)
        .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    var config = builder.Configuration;
    config.AddEnvironmentVariables("PhotoPortfolio_");

    builder.Host.UseSerilog();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    builder.Services.Configure<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.NameClaimType = "name";
        });

    builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

    builder.Services.AddOcelot(builder.Configuration)
                    .AddCacheManager(settings => settings.WithDictionaryHandle());

    builder.Services.ConfigureOcelotPlaceholders(builder.Configuration); // custom extension: ./Helpers/FileConfigurationExtensions

    builder.Services.AddSingleton(_ =>
        new MongoContext(builder.Configuration.GetValue<string>("MongoConnection:ConnectionString"),
                         builder.Configuration.GetValue<string>("MongoConnection:DatabaseName")));

    builder.Services.AddScoped<IGalleryRepository, GalleryRepository>();
    builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IQuoteService, QuoteService>();
    builder.Services.AddScoped<IPreferencesRepository, PreferencesRepository>();
    builder.Services.AddScoped<ICreditService, CreditService>();
    builder.Services.AddScoped<IUploadService, UploadService>();
    builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<IStripeClientFactory, StripeClientFactory>();

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

    builder.Services.AddRazorPages();

    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();

        // Seed initial data
        if (config.GetValue<bool>("InitialDataSeeding"))
        {
            await SeedInitialData(app, config);
        }
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.MapWhen(context => context.Request.Path.StartsWithSegments("/prodigi"), appBuilder =>
    {
        appBuilder.UseOcelot();
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task SeedInitialData(IHost app, IConfiguration config)
{
    try
    {
        var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

        if (scopedFactory == null)
        {
            Log.Error("An error occurred while seeding initial data: ServiceScopeFactory is null");
            return;
        }

        using var scope = scopedFactory.CreateScope();
        var appContext = scope.ServiceProvider.GetService<MongoContext>();

        if (appContext == null)
        {
            Log.Error("An error occurred while seeding initial data: MongoContext is null");
            return;
        }

        var databaseName = config.GetValue<string>("MongoConnection:DatabaseName");

        /// optionally check if the database already exists
        //var databases = await appContext.Client.ListDatabaseNamesAsync().Result.ToListAsync();
        //if (!databases.Contains(databaseName))
        //{
            appContext.Client.GetDatabase(databaseName);
            await SeedData.SeedDatabaseCollections(appContext, config);
        //}
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding initial data");
    }
}
