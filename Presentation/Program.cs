using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Presentation.Interfaces;
using Presentation.ServiceBus;
using Presentation.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<EmailClient>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("ACS");
    return new EmailClient(connectionString);
});
builder.Services.AddTransient<IVerificationService, VerificationService>();

var app = builder.Build();

app.MapGet("/", () => "EmailService is running!");

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();





