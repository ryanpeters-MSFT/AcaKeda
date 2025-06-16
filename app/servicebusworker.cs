using Azure.Messaging.ServiceBus;

public class ServiceBusWorker(IConfiguration config, ILogger<ServiceBusWorker> logger) : BackgroundService
{
    private readonly string _connectionString = config["SERVICE_BUS_CONNECTION_STRING"];
    private readonly string _queueName = config["ServiceBus:Queue"];
    private ServiceBusProcessor _processor;
    private ServiceBusClient _client;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _client = new ServiceBusClient(_connectionString);

        _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(cancellationToken);

        logger.LogInformation("Service Bus processor started.");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // No need to do anything here; processor runs in background.
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        logger.LogInformation("Service Bus processor stopped.");

        await base.StopAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();

        logger.LogInformation($"Received message: {body}");

        await Task.Delay(5000); // Simulate some processing delay

        // Process message here

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Message handler encountered an exception");

        return Task.CompletedTask;
    }
}
