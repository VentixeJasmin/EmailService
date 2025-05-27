namespace Presentation.ServiceBus;

public class UserRegisteredEvent : BaseEvent
{
    public string Email { get; set; } = null!;
    public string EventType => "UserRegistered";
}
