using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Distributed;
using Presentation.Interfaces;
using Presentation.Models;

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
        var plainTextContent = $"Follow this link and enter the code {code} to verify your email address.";
        var htmlContent = @$"
		<html>
			<body>
				<h1>Follow this <a href="">link</a> and enter the code {code} to verify your email address.</h1>
			</body>
		</html>";
        var emailMessage = new EmailMessage(
            senderAddress: _configuration["ACS: SenderAddress"],
            recipients: new EmailRecipients([new(request.Email)]),
            content: new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            }
        );

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
        var storedCode = _cache.Get(key);

        if (storedCode != null) 
        {
            if (storedCode.Equals(request.Code))
            {
                _cache.Remove(key);
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
}
