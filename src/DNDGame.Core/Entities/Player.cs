namespace DNDGame.Core.Entities;

public class Player
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<Character> Characters { get; set; } = [];
}
