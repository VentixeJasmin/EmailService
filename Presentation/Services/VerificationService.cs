using Azure.Communication.Email;
using Presentation.Interfaces;
using Presentation.Models;

namespace Presentation.Services;

public class VerificationService(IConfiguration configuration, EmailClient emailClient) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;

    private static readonly Random _random = new();

    public Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task SaveVerificationCodeAsync(SaveVerificationCodeRequest request)
    {
        throw new NotImplementedException();
    }

    public VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request)
    {
        throw new NotImplementedException();
    }
}
