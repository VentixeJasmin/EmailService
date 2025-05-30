using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Distributed;
using Presentation.Interfaces;
using Presentation.Models;
using StackExchange.Redis;
using System.Net.Sockets;

namespace Presentation.Services;

public class VerificationService(IConfiguration configuration, EmailClient emailClient, IDistributedCache cache) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IDistributedCache _cache = cache;

    private static readonly Random _random = new();

    public async Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        if (request == null || String.IsNullOrWhiteSpace(request.Email))
        {
            return new VerificationServiceResult
            {
                Succeeded = false,
                Error = "Recipient email address is required."
            };
        }

        var code = _random.Next(100000, 999999).ToString();
        var subject = "Your verification code from Ventixe";
        var plainTextContent = $"Hello {request.FirstName} {request.LastName}! Follow the the link and enter the code {code} to verify your email address. Link: https://jolly-ocean-090980503.6.azurestaticapps.net/verify?email={request.Email}";
        
        //ChatGPT 4o helped me with the email styling here.  
        var htmlContent = @$"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Verification</title>
</head>
<body style=""margin:0; padding:0; background-color:#ffffff; font-family: Inter, Arial, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; padding:2rem;"">
        <tr>
            <td>
                <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ededed; padding:1rem; margin:1rem auto;"">
                    <tr>
                        <td>
                            <h2 style=""color:#F26CF9; margin:0 0 1rem;"">Hello {request.FirstName} {request.LastName} - Welcome to Ventixe</h2>
                            <p style=""color:#434345; margin:0;"">Before you can start managing events, we need to verify your email address.</p>
                        </td>
                    </tr>
                </table>

                <table width=""600"" align=""center"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ededed; padding:1rem; margin:1rem auto;"">
                    <tr>
                        <td>
                            <p style=""color:#434345; margin:0;"">
                                Follow this <a href=""https://jolly-ocean-090980503.6.azurestaticapps.net/verify?email={request.Email}"" style=""color:#5562A2;"">link</a> and enter the code 
                                <span style=""color:#5562A2; font-size:24px; font-weight:bold;"">{code}</span> to verify your email address.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        var emailMessage = new EmailMessage(
            senderAddress: _configuration["ACS:SenderAddress"],
            recipients: new EmailRecipients([new(request.Email)]),
            content: new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            }
        );

        Console.WriteLine("=== EMAIL PROCESSING COMPLETE ===");

        try
        {
            var emailSendOperation = await _emailClient.SendAsync(Azure.WaitUntil.Started, emailMessage);
            await SaveVerificationCodeAsync(new SaveVerificationCodeRequest
            {
                Email = request.Email,
                Code = code,
                ValidFor = TimeSpan.FromMinutes(5)
            });
            return new VerificationServiceResult
            {
                Succeeded = true
            }; 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new VerificationServiceResult
            {
                Succeeded = false,
                Error = ex.ToString()
            }; 
        }
    }

    public async Task SaveVerificationCodeAsync(SaveVerificationCodeRequest request)
    {
        await _cache.SetStringAsync(request.Email.ToLowerInvariant(), request.Code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.ValidFor
        });

        return;
    }

    public VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request)
    {
        var key = request.Email.ToLowerInvariant();

        try
        {
            var storedCode = _cache.GetString(key);

            if (storedCode != null)
            {
                if (storedCode.Equals(request.Code))
                {
                    try
                    {
                        _cache.Remove(key);
                    }
                    catch (RedisConnectionException)
                    {

                    }

                    return new VerificationServiceResult
                    {
                        Succeeded = true
                    };
                }
            }

            return new VerificationServiceResult
            {
                Succeeded = false,
                Error = "Invalid or expired verification code."
            };
        }
        catch (RedisConnectionException)
        {
            // Maybe retry once or return a specific error
            return new VerificationServiceResult
            {
                Succeeded = false,
                Error = "Verification service temporarily unavailable. Please try again."
            };
        }
    }
}
