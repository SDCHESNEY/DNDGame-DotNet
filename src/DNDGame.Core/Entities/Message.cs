using DNDGame.Core.Enums;

namespace DNDGame.Core.Entities;

public class Message
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public required string AuthorId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Session Session { get; set; } = null!;
}
