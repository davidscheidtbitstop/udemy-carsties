using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine($"--> Consuming AuctionCreatedFaultConsumer: {context.Message.Message.Id}");
        var exception = context.Message.Exceptions.First();

        if (exception is ArgumentException validationException)
        {
            context.Message.Message.Model = "FooBar";
            await context.Publish(context.Message.Message);
        }
        
    }
}