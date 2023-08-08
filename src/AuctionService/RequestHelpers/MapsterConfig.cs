using System.Reflection;
using AuctionService.DTOs;
using AuctionService.Models;
using Mapster;

namespace AuctionService.RequestHelpers;

public static class MapsterConfig
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig<Auction, AuctionDto>
            .NewConfig()
            .Map(dest => dest, src => src.Item);
        TypeAdapterConfig<CreateAuctionDto, Auction>
            .NewConfig()
            .Map(dest => dest.Item, src => src);

        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}
