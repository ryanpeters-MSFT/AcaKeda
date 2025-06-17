using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

public class StorageQueueWorker(IConfiguration config, ILogger<ServiceBusWorker> logger, QueueClient queueClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 5, visibilityTimeout: TimeSpan.FromMinutes(1), cancellationToken: stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    // Process the message
                    logger.LogInformation($"Processing message: {message.MessageText}");

                    await Task.Delay(5000); // Simulate some processing delay

                    // TODO: Add your message processing logic here

                    // Delete the message after successful processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                    // Optionally, handle poison messages or retry logic here
                }
            }

            // Wait before polling again (adjust as needed)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
