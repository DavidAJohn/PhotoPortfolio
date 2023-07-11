namespace PhotoPortfolio.Server.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _config;

    public ConfigurationService(IConfiguration config)
    {
        _config = config;
    }

    public IConfiguration GetConfiguration()
    {
        return _config;
    }
}
