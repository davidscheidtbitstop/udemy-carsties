using Contracts;
using Mapster;
using MassTransit;
using MongoDB.Entities;
using SearchService.Model;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"--> Consuming AuctionCreatedConsumer: {context.Message.Id}");
        
        var item = context.Message.Adapt<Item>();
        
        await item.SaveAsync();
    }
}