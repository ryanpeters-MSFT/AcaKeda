using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["SERVICE_BUS_CONNECTION_STRING"];;
    return new ServiceBusClient(connectionString);
});

builder.Services.AddHostedService<ServiceBusWorker>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();