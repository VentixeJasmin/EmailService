namespace Presentation.ServiceBus;

public class VerificationCodeSentEvent : BaseEvent
{
    public string Email { get; set; } = null!;
    public string EventType => "VerificationCodeSent";
}
