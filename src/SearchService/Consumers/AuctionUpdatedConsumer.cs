using Contracts;
using Mapster;
using MassTransit;
using MongoDB.Entities;
using SearchService.Model;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer: IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"--> Consuming AuctionUpdateConsumer: {context.Message.Id}");

        var item = context.Message.Adapt<Item>();
        item.UpdatedAt = DateTime.UtcNow;

        var result = await DB.Update<Item>()
            .Match(a => a.ID == context.Message.Id)
            .ModifyOnly(i => new
            {
                i.Make,
                i.Model,
                i.Year,
                i.Color,
                i.Mileage,
                i.UpdatedAt
            }, item).ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            throw new Exception($"Could not update item: {context.Message.Id}");
        }
    }
}