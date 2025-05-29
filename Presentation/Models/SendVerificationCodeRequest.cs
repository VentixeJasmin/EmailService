using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class SendVerificationCodeRequest
{
    [Required]
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}
