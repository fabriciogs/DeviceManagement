using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DeviceManagement.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddAppSettingsConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services;
    }

    private static TClass GetFromAppSettings<TClass>(this IConfiguration config) where TClass : class, new()
        => config.GetSection($"AppSettings:{typeof(TClass).Name}").Get<TClass>() ?? new TClass();
}