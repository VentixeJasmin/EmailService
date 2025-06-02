using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Presentation.Interfaces;
using Presentation.ServiceBus;
using Presentation.Services;



var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

//Got help with the ServiceBus configuring by Claude AI
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<Program>>();
    var connectionString = builder.Configuration.GetConnectionString("ServiceBus");
    logger.LogInformation($"ServiceBus connection string exists: {!string.IsNullOrEmpty(connectionString)}");
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

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== EMAIL SERVICE STARTING ===");

app.MapGet("/", () => "EmailService is running!");

logger.LogInformation("=== EMAIL SERVICE CONFIGURED, STARTING APP ===");
app.Run();


app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

logger.LogInformation("Email service is starting...");

app.Run();





