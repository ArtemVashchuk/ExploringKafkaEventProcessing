using System.Text;
using System.Text.Json;
using EventStore.Client;

namespace ExploringKafkaEventProcessing;

public class EventStoreService(EventStoreClient client)
{
    public async Task AppendEventAsync(string streamId, object @event)
    {
        var jsonData = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        var eventData = new EventData(Uuid.NewUuid(), @event.GetType().Name, jsonData);

        await client.AppendToStreamAsync(streamId, StreamState.Any, [eventData]);
    }

    public async Task<List<object>> ReadEventsAsync(string streamId)
    {
        var events = new List<object>();
        var result = client.ReadStreamAsync(Direction.Forwards, streamId, StreamPosition.Start);

        await foreach (var resolvedEvent in result)
        {
            var json = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var eventType = resolvedEvent.Event.EventType;

            if (eventType == nameof(MoneyDeposited))
            {
                var moneyDeposit = JsonSerializer.Deserialize<MoneyDeposited>(json);

                if (moneyDeposit != null)
                {
                    events.Add(moneyDeposit);
                }
            }
        }

        return events;
    }
    
    public async Task<BankAccount> GetBankAccountAsync(string streamId)
    {
        var bankAccount = new BankAccount();
        var result = client.ReadStreamAsync(Direction.Forwards, streamId, StreamPosition.Start);

        await foreach (var resolvedEvent in result)
        {
            var json = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var eventType = resolvedEvent.Event.EventType;

            if (eventType == nameof(MoneyDeposited))
            {
                var moneyDeposit = JsonSerializer.Deserialize<MoneyDeposited>(json);

                if (moneyDeposit != null)
                {
                    bankAccount.Apply(moneyDeposit);
                }
            }
        }

        return bankAccount;
    }
}