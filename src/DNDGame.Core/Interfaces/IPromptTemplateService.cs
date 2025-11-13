using DNDGame.Core.Enums;
using DNDGame.Core.Models;

namespace DNDGame.Core.Interfaces;

/// <summary>
/// Interface for prompt template service.
/// Manages system prompts and context formatting for the LLM.
/// </summary>
public interface IPromptTemplateService
{
    /// <summary>
    /// Gets the system prompt for the Dungeon Master role.
    /// </summary>
    /// <param name="sessionMode">The session mode (solo/multiplayer).</param>
    /// <returns>The system prompt defining DM behavior.</returns>
    string GetSystemPrompt(SessionMode sessionMode);

    /// <summary>
    /// Gets the combat-specific prompt with current combat state.
    /// </summary>
    /// <param name="context">The session context.</param>
    /// <returns>Combat prompt with state information.</returns>
    string GetCombatPrompt(SessionContext context);

    /// <summary>
    /// Gets the exploration prompt for non-combat scenarios.
    /// </summary>
    /// <param name="context">The session context.</param>
    /// <returns>Exploration prompt.</returns>
    string GetExplorationPrompt(SessionContext context);

    /// <summary>
    /// Gets the NPC dialogue prompt.
    /// </summary>
    /// <param name="npc">The NPC context.</param>
    /// <param name="playerMessage">The player's message.</param>
    /// <returns>NPC dialogue prompt.</returns>
    string GetNpcPrompt(NpcContext npc, string playerMessage);

    /// <summary>
    /// Gets the scene description prompt.
    /// </summary>
    /// <param name="location">The location context.</param>
    /// <returns>Scene description prompt.</returns>
    string GetScenePrompt(LocationContext location);

    /// <summary>
    /// Formats the session context into a string for the LLM.
    /// </summary>
    /// <param name="context">The session context.</param>
    /// <returns>Formatted context string.</returns>
    string FormatContext(SessionContext context);
}
