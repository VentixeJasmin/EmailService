namespace Presentation.ServiceBus;

public class UserRegisteredEvent : BaseEvent
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!; 
    public string EventType { get; set; } = "UserRegistered";
}
