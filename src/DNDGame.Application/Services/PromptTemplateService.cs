using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.Models;
using System.Text;

namespace DNDGame.Application.Services;

/// <summary>
/// Service for generating and managing LLM prompt templates.
/// Formats context and creates appropriate prompts for different game scenarios.
/// </summary>
public class PromptTemplateService : IPromptTemplateService
{
    private const string BaseSystemPrompt = @"You are an expert Dungeon Master for a Dungeons & Dragons 5th Edition game. 
You create immersive, engaging narratives while following D&D rules accurately.

Guidelines:
- Describe scenes vividly using all five senses
- Give players meaningful choices and consequences
- Never control player characters' actions or thoughts
- Maintain consistency with established facts
- Use appropriate D&D terminology
- Keep responses concise (2-3 paragraphs maximum)
- End with a clear question or prompt for player action";

    public string GetSystemPrompt(SessionMode sessionMode)
    {
        var prompt = new StringBuilder(BaseSystemPrompt);

        prompt.AppendLine();
        prompt.AppendLine();

        if (sessionMode == SessionMode.Solo)
        {
            prompt.AppendLine("This is a solo adventure. You should:");
            prompt.AppendLine("- Focus deeply on the single player character's experience");
            prompt.AppendLine("- Provide more detailed descriptions and internal moments");
            prompt.AppendLine("- Adjust difficulty to maintain challenge without overwhelming");
        }
        else
        {
            prompt.AppendLine("This is a multiplayer adventure. You should:");
            prompt.AppendLine("- Give each player character opportunities to shine");
            prompt.AppendLine("- Encourage party cooperation and teamwork");
            prompt.AppendLine("- Balance attention across all characters");
            prompt.AppendLine("- Create challenges that require different skills");
        }

        return prompt.ToString();
    }

    public string GetCombatPrompt(SessionContext context)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("COMBAT SCENARIO");
        prompt.AppendLine("The party is currently in combat. Narrate the action dramatically.");
        prompt.AppendLine();
        
        if (context.ActiveCharacters.Any())
        {
            prompt.AppendLine("Active Characters:");
            foreach (var character in context.ActiveCharacters)
            {
                prompt.AppendLine($"- {character.Name} (Level {character.Level} {character.Class})");
                prompt.AppendLine($"  HP: {character.HitPoints}/{character.MaxHitPoints}, AC: {character.ArmorClass}");
            }
            prompt.AppendLine();
        }

        if (!string.IsNullOrEmpty(context.CurrentScene))
        {
            prompt.AppendLine($"Combat Location: {context.CurrentScene}");
            prompt.AppendLine();
        }

        prompt.AppendLine("Describe combat outcomes, environmental effects, and tactical options.");
        prompt.AppendLine("Keep the action moving and exciting!");

        return prompt.ToString();
    }

    public string GetExplorationPrompt(SessionContext context)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine("EXPLORATION SCENARIO");
        prompt.AppendLine("The party is exploring and discovering their surroundings.");
        prompt.AppendLine();

        if (!string.IsNullOrEmpty(context.CurrentScene))
        {
            prompt.AppendLine($"Current Location: {context.CurrentScene}");
            prompt.AppendLine();
        }

        if (context.ActiveCharacters.Any())
        {
            prompt.AppendLine("Party Members:");
            foreach (var character in context.ActiveCharacters)
            {
                prompt.AppendLine($"- {character.Name} (Level {character.Level} {character.Class})");
            }
            prompt.AppendLine();
        }

        prompt.AppendLine("Describe the environment in vivid detail.");
        prompt.AppendLine("Include interesting points of interaction and potential discoveries.");
        prompt.AppendLine("Hint at dangers or opportunities without revealing everything.");

        return prompt.ToString();
    }

    public string GetNpcPrompt(NpcContext npc, string playerMessage)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"You are roleplaying as {npc.Name}, an NPC in a D&D game.");
        prompt.AppendLine();
        prompt.AppendLine($"Personality: {npc.PersonalityTraits}");

        if (npc.HasOccupation)
        {
            prompt.AppendLine($"Occupation: {npc.Occupation}");
        }

        if (npc.HasMood)
        {
            prompt.AppendLine($"Current Mood: {npc.CurrentMood}");
        }

        prompt.AppendLine();
        prompt.AppendLine("Guidelines:");
        prompt.AppendLine("- Stay in character at all times");
        prompt.AppendLine("- Speak with a distinct voice matching your personality");
        prompt.AppendLine("- React realistically to what the player says");
        prompt.AppendLine("- Keep responses conversational (1-2 paragraphs)");
        prompt.AppendLine("- Don't break the fourth wall or reference game mechanics");
        prompt.AppendLine();
        prompt.AppendLine($"The player says: \"{playerMessage}\"");
        prompt.AppendLine();
        prompt.AppendLine("Respond as this NPC:");

        return prompt.ToString();
    }

    public string GetScenePrompt(LocationContext location)
    {
        var prompt = new StringBuilder();
        
        prompt.AppendLine($"Describe the location: {location.Name}");
        prompt.AppendLine($"Type: {location.LocationType}");
        prompt.AppendLine();

        if (location.HasDescription)
        {
            prompt.AppendLine($"Background: {location.Description}");
            prompt.AppendLine();
        }

        if (location.HasFeatures)
        {
            prompt.AppendLine("Notable Features:");
            foreach (var feature in location.VisibleFeatures)
            {
                prompt.AppendLine($"- {feature}");
            }
            prompt.AppendLine();
        }

        if (location.HasNpcs)
        {
            prompt.AppendLine("NPCs Present:");
            foreach (var npcName in location.PresentNpcs)
            {
                prompt.AppendLine($"- {npcName}");
            }
            prompt.AppendLine();
        }

        prompt.AppendLine("Guidelines:");
        prompt.AppendLine("- Create a vivid, immersive description (2-3 paragraphs)");
        prompt.AppendLine("- Engage multiple senses (sight, sound, smell, touch)");
        prompt.AppendLine("- Establish atmosphere and mood");
        prompt.AppendLine("- Highlight interesting details for player exploration");
        prompt.AppendLine("- Don't dictate player actions or feelings");

        return prompt.ToString();
    }

    public string FormatContext(SessionContext context)
    {
        var formatted = new StringBuilder();
        
        formatted.AppendLine("=== GAME CONTEXT ===");
        formatted.AppendLine();

        if (!string.IsNullOrEmpty(context.CurrentScene))
        {
            formatted.AppendLine($"Current Scene: {context.CurrentScene}");
            formatted.AppendLine();
        }

        if (context.ActiveCharacters.Any())
        {
            formatted.AppendLine("Party:");
            foreach (var character in context.ActiveCharacters)
            {
                formatted.AppendLine($"- {character.Name}: Level {character.Level} {character.Class}");
                formatted.AppendLine($"  HP: {character.HitPoints}/{character.MaxHitPoints}, AC: {character.ArmorClass}");
                
                if (character.Skills.Any())
                {
                    formatted.AppendLine($"  Proficient Skills: {string.Join(", ", character.Skills)}");
                }
            }
            formatted.AppendLine();
        }

        if (context.WorldFlags.Any())
        {
            formatted.AppendLine("Important Story Flags:");
            foreach (var flag in context.WorldFlags)
            {
                formatted.AppendLine($"- {flag.Key}: {flag.Value}");
            }
            formatted.AppendLine();
        }

        if (context.RecentMessages.Any())
        {
            formatted.AppendLine("Recent Events:");
            var recentCount = Math.Min(5, context.RecentMessages.Count);
            foreach (var message in context.RecentMessages.TakeLast(recentCount))
            {
                var roleLabel = message.Role == MessageRole.Player ? "PLAYER" : "DM";
                formatted.AppendLine($"[{roleLabel}] {message.Content}");
            }
            formatted.AppendLine();
        }

        formatted.AppendLine("=== END CONTEXT ===");

        return formatted.ToString();
    }
}
