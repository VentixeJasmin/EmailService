using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Presentation.Interfaces;
using Presentation.Models;
using Presentation.ServiceBus;
using System.Text.Json;

namespace Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VerificationController(IVerificationService verificationService, ServiceBusClient serviceBusClient) : ControllerBase
{
    private readonly IVerificationService _verificationService = verificationService;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;


    [HttpPost("send")]
    public async Task<IActionResult> Send(SendVerificationCodeRequest req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { Error = "Recipient email address is required." });
        }

        var result = await _verificationService.SendVerificationCodeAsync(req);
        if (result.Succeeded)
        {
            await PublishVerificationSentEvent(req.Email);
        }

        return result.Succeeded ? Ok(result) : StatusCode(500, result);
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyVerificationCodeRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Invalid or expired verification code." });

        var result = _verificationService.VerifyVerificationCode(req);
        if (result.Succeeded)
        {
            await PublishEmailVerifiedEvent(req.Email);
        }

        return result.Succeeded ? Ok(result) : StatusCode(500, result);
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

    private async Task PublishEmailVerifiedEvent(string email)
    {
        var sender = _serviceBusClient.CreateSender("account-created");
        var eventMessage = new EmailVerifiedEvent
        {
            Email = email
        };

        var message = new ServiceBusMessage(JsonSerializer.Serialize(eventMessage));

        await sender.SendMessageAsync(message);
    }
}
