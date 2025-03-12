using Confluent.Kafka;

namespace ExploringKafkaEventProcessing;

public class KafkaEventConsumer : BackgroundService
{
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly IConsumer<string, string> _kafkaConsumer;

    public KafkaEventConsumer(ILogger<KafkaEventConsumer> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "bank-account-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _kafkaConsumer = new ConsumerBuilder<string, string>(config).Build();
        _kafkaConsumer.Subscribe("bank-transactions");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _kafkaConsumer.Consume(stoppingToken);
                    ProcessEvent(consumeResult.Message.Key, consumeResult.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka event");
                }
            }
        });
    }

    private void ProcessEvent(string key, string value)
    {
        _logger.LogInformation($"Processing event for {key}: {value}");
    }

    public override void Dispose()
    {
        _kafkaConsumer.Close();
        _kafkaConsumer.Dispose();
    }
}