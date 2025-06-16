using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

var serviceBusConnectionString = Environment.GetEnvironmentVariable("SERVICE_BUS_CONNECTION_STRING");

Console.WriteLine($"SERVICE_BUS_CONNECTION_STRING = {serviceBusConnectionString}");

builder.Services.AddControllers();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new ServiceBusClient(serviceBusConnectionString);
});

builder.Services.AddHostedService<ServiceBusWorker>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();