using Azure.Messaging.ServiceBus;
using Presentation.Interfaces;
using Presentation.Models;
using System.Text.Json;

namespace Presentation.ServiceBus;

public class AccountCreatedMessageHandler : BackgroundService
//Claude AI generated the base for this handler and I have modified it some. 
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AccountCreatedMessageHandler> _logger;
    private readonly ServiceBusClient _serviceBusClient;


    public AccountCreatedMessageHandler(ServiceBusClient serviceBusClient, IServiceProvider serviceProvider, ILogger<AccountCreatedMessageHandler> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _logger.LogInformation("AccountCreatedMessageHandler constructor called"); // Add this

        _processor = serviceBusClient.CreateProcessor("account-created", "email");
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ServiceBus message processing..."); // Add this
        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("ServiceBus message processing started successfully"); // Add this
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageBody = args.Message.Body.ToString();
            var baseEvent = JsonSerializer.Deserialize<JsonElement>(messageBody);
            var eventType = baseEvent.GetProperty("EventType").GetString();

            if (eventType == "UserRegistered")
            {
                var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(messageBody);

                using var scope = _serviceProvider.CreateScope();
                var verificationService = scope.ServiceProvider.GetRequiredService<IVerificationService>();

                var result = await verificationService.SendVerificationCodeAsync(new SendVerificationCodeRequest
                {
                    Email = userRegisteredEvent!.Email!,
                    FirstName = userRegisteredEvent.FirstName,
                    LastName = userRegisteredEvent.LastName
                });

                if (result.Succeeded)
                {
                    await args.CompleteMessageAsync(args.Message);
                    await PublishVerificationSentEvent(userRegisteredEvent.Email);
                }
            }
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error processing user event message");
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private async Task PublishVerificationSentEvent(string email)
    {
        var sender = _serviceBusClient.CreateSender("account-created");
        var eventMessage = new VerificationCodeSentEvent
        {
            Email = email
        };

        var message = new ServiceBusMessage(JsonSerializer.Serialize(eventMessage));

        await sender.SendMessageAsync(message);
    }
}

