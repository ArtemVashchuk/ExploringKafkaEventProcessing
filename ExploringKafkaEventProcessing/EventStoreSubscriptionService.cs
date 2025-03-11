using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace ExploringKafkaEventProcessing;

public class EventStoreSubscriptionService(EventStoreClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await client.SubscribeToStreamAsync(
            "bank-account-1",
            FromStream.Start,
            async (_, @event, _) => await HandleMoneyDepositedEvent(@event),
            cancellationToken: cancellationToken
        );
    }

    private Task HandleMoneyDepositedEvent(ResolvedEvent resolvedEvent)
    {
        var json = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
        var @event = JsonSerializer.Deserialize<MoneyDeposited>(json);
        Console.WriteLine($"New deposit detected: {@event?.Amount}");

        return Task.CompletedTask;
    }
}