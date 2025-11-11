using DNDGame.Core.Enums;

namespace DNDGame.Core.Entities;

public class Session
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public SessionMode Mode { get; set; }
    public SessionState State { get; set; } = SessionState.Created;
    public string? CurrentScene { get; set; }
    public int? CurrentTurnCharacterId { get; set; }
    public string WorldFlags { get; set; } = "{}"; // JSON serialized dictionary
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastActivityAt { get; set; }
    
    // Navigation properties
    public List<Message> Messages { get; set; } = [];
    public List<DiceRoll> DiceRolls { get; set; } = [];
}
