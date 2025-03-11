using EventStore.Client;
using ExploringKafkaEventProcessing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ =>
{
    var settings = EventStoreClientSettings.Create("esdb://localhost:2113?tls=false");

    return new EventStoreClient(settings);
});

builder.Services.AddSingleton<EventStoreService>();
builder.Services.AddHostedService<EventStoreSubscriptionService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Event Store API is Running!");

app.MapPost("/deposit", async (EventStoreService service, decimal amount) =>
{
    var depositEvent = new MoneyDeposited(Guid.NewGuid(), amount);
    await service.AppendEventAsync("bank-account-1", depositEvent);

    return Results.Ok("Deposit recorded");
});

app.MapGet("/transactions", async (EventStoreService service) =>
{
    var events = await service.ReadEventsAsync("bank-account-1");

    return Results.Ok(events);
});

app.MapGet("/balance", async (EventStoreService service) =>
{
    var balance = await service.GetBankAccountAsync("bank-account-1");

    return Results.Ok(balance);
});

app.Run();