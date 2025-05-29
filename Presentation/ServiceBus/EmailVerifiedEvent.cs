namespace Presentation.ServiceBus;

public class EmailVerifiedEvent : BaseEvent
{
    public string Email { get; set; } = null!;
    public string EventType { get; set; } = "EmailVerified";
}