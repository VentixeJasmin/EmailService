using Presentation.Models;

namespace Presentation.Interfaces;

public interface IVerificationService
{
    Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request);
    Task SaveVerificationCodeAsync(SaveVerificationCodeRequest request);
    VerificationServiceResult VerifyVerificationCode(VerifyVerificationCodeRequest request); 
}
