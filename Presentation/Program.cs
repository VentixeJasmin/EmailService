using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Presentation.Interfaces;
using Presentation.ServiceBus;
using Presentation.Services;


var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== EMAIL SERVICE STARTING ===");
Console.WriteLine($"SenderAddress: {builder.Configuration["SenderAddress"]}");
Console.WriteLine($"ServiceBus: {builder.Configuration.GetConnectionString("ServiceBus")}");
Console.WriteLine($"All config keys: {string.Join(", ", builder.Configuration.AsEnumerable().Select(x => x.Key))}");
Console.WriteLine("=== EMAIL SERVICE CONFIG CHECK DONE ===");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Got help with the ServiceBus configuring by Claude AI
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("ServiceBus");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddHostedService<AccountCreatedMessageHandler>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
builder.Services.AddSingleton(x => new EmailClient(builder.Configuration["ConnectionStrings:ACS"])); 
builder.Services.AddTransient<IVerificationService, VerificationService>();

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Email service is starting...");

app.Run();





