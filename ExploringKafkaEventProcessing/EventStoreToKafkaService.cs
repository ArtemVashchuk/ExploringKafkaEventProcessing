using System.Text;
using Confluent.Kafka;
using EventStore.Client;

namespace ExploringKafkaEventProcessing;

public class EventStoreToKafkaService : BackgroundService
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly ILogger<EventStoreToKafkaService> _logger;

    public EventStoreToKafkaService(
        EventStoreClient eventStoreClient,
        ILogger<EventStoreToKafkaService> logger)
    {
        _eventStoreClient = eventStoreClient;
        _logger = logger;

        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        _kafkaProducer = new ProducerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _eventStoreClient.SubscribeToStreamAsync(
            "bank-account-1",
            FromStream.Start, 
            (_, @event, _) => PublishEventToKafka(@event),
            cancellationToken: stoppingToken);
    }

    private async Task PublishEventToKafka(ResolvedEvent resolvedEvent)
    {
        var key = resolvedEvent.Event.EventStreamId;
        var value = Encoding.UTF8.GetString(resolvedEvent.Event.Data.ToArray());

        await _kafkaProducer.ProduceAsync("bank-transactions",
            new Message<string, string> { Key = key, Value = value });
        
        _logger.LogInformation($"Published event {resolvedEvent.Event.EventType} to Kafka");
    }
}