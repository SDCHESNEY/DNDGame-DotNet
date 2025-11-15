namespace DNDGame.Core.Models;

/// <summary>
/// Represents the initiative order for a combat encounter.
/// </summary>
public record InitiativeOrder
{
    public required List<InitiativeEntry> Entries { get; init; }
    public int CurrentTurnIndex { get; init; }
    
    public InitiativeEntry? CurrentTurn => 
        Entries.Any() && CurrentTurnIndex < Entries.Count 
            ? Entries[CurrentTurnIndex] 
            : null;
}
