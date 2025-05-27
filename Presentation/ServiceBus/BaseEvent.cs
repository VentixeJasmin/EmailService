namespace Presentation.ServiceBus; 

public abstract class BaseEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventId { get; set; } = Guid.NewGuid().ToString();
}
