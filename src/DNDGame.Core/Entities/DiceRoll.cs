using DNDGame.Core.Enums;

namespace DNDGame.Core.Entities;

public class DiceRoll
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public required string RollerId { get; set; }
    public required string Formula { get; set; }
    public string IndividualRolls { get; set; } = "[]"; // JSON serialized array
    public int Modifier { get; set; }
    public int Total { get; set; }
    public DiceRollType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Session Session { get; set; } = null!;
}
