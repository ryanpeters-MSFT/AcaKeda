using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;

namespace KedaApp.Controllers;

[ApiController]
[Route("/")]
public class KedaController(ILogger<KedaController> logger, IConfiguration config, ServiceBusClient serviceBusClient, QueueClient queueClient) : ControllerBase
{
    private readonly string _queueName = config["ServiceBusName"];

    [HttpGet, Route("longrunning")]
    public async Task<ContentResult> LongRunning()
    {
        await Task.Delay(5000); // Simulate a long-running operation
        logger.LogInformation("Long-running operation completed.");

        return Content("Long-running operation completed successfully.");
    }

    [HttpGet, Route("servicebusmessage")]
    public async Task<ContentResult> ServiceBusMessage()
    {
        // Create a sender for the queue
        ServiceBusSender sender = serviceBusClient.CreateSender(_queueName);

        // Create a message to send
        ServiceBusMessage message = new ServiceBusMessage("Hello, KEDA!");

        // Send the message
        await sender.SendMessageAsync(message);

        // Optionally, dispose the sender
        await sender.DisposeAsync();

        return Content("Message sent to service bus queue");
    }

    [HttpGet, Route("storagequeuemessage")]
    public async Task<ContentResult> StorageQueueMessage()
    {
        // Create a sender for the queue
        ServiceBusSender sender = serviceBusClient.CreateSender(_queueName);

        // Create a message to send
        var response = await queueClient.SendMessageAsync("Hello, Azure Queue!");
       
        return Content("Message sent to storage queue");
    }
}
