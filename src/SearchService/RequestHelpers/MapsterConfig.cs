using System.Reflection;
using Mapster;

namespace SearchService.RequestHelpers;

public static class MapsterConfig
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.Flexible);
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
    
}