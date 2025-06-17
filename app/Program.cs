using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("ServiceBus");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("StorageQueue");
    var queueName = config["StorageQueueName"];
    return new QueueClient(connectionString, queueName);
});

builder.Services.AddHostedService<ServiceBusWorker>();
builder.Services.AddHostedService<StorageQueueWorker>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();