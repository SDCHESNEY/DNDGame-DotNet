# LLM Integration Guide

Complete guide for integrating and configuring the AI Dungeon Master in DNDGame.

---

## Table of Contents

1. [Overview](#overview)
2. [OpenAI API Setup](#openai-api-setup)
3. [Configuration](#configuration)
4. [API Endpoints](#api-endpoints)
5. [Prompt Customization](#prompt-customization)
6. [Content Moderation](#content-moderation)
7. [Streaming Responses](#streaming-responses)
8. [Testing](#testing)
9. [Cost Management](#cost-management)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The LLM integration uses OpenAI's GPT models to power an AI Dungeon Master that:

- **Generates narrative responses** to player actions
- **Maintains conversation context** across multiple turns
- **Adapts prompts** based on game mode (combat vs exploration)
- **Creates NPC dialogue** with personality traits
- **Describes scenes** with sensory details
- **Moderates content** to filter inappropriate input/output
- **Handles failures gracefully** with retry policies

### Architecture

```
Player Action
    ↓
Content Moderation (Input)
    ↓
Context Building (Session + Messages)
    ↓
Prompt Template Selection (Combat/Exploration/NPC/Scene)
    ↓
OpenAI API Call (with Polly retry)
    ↓
Content Moderation (Output)
    ↓
Suggested Actions Extraction
    ↓
DM Response → Player
```

---

## OpenAI API Setup

### 1. Create OpenAI Account

1. Go to [platform.openai.com](https://platform.openai.com/)
2. Sign up or log in
3. Navigate to **API Keys** section
4. Click **Create new secret key**
5. Copy the key (starts with `sk-`)
6. **Store it securely** - you won't be able to see it again

### 2. Configure Billing

1. Go to **Billing** → **Payment methods**
2. Add a payment method
3. Set **usage limits** to prevent unexpected charges:
   - **Hard limit**: $50/month (recommended for development)
   - **Soft limit**: $25/month (get email alert)

### 3. Model Access

Ensure you have access to:
- ✅ `gpt-4-turbo-preview` (best quality, higher cost)
- ✅ `gpt-3.5-turbo` (faster, lower cost)

Check access at **Settings** → **Models**.

---

## Configuration

### Development Environment

#### Option 1: appsettings.Development.json

```json
{
  "LLM": {
    "Provider": "OpenAI",
    "ApiKey": "sk-your-api-key-here",
    "Model": "gpt-4-turbo-preview",
    "BaseUrl": "https://api.openai.com/v1",
    "MaxTokens": 500,
    "Temperature": 0.7,
    "StreamResponses": true
  },
  "ContentModeration": {
    "Enabled": true,
    "BlockNSFW": true,
    "BlockHarassment": true,
    "MaxInputLength": 2000
  }
}
```

#### Option 2: Environment Variables

```bash
# Unix/macOS
export LLM__ApiKey="sk-your-api-key-here"
export LLM__Model="gpt-4-turbo-preview"
export LLM__MaxTokens="500"
export LLM__Temperature="0.7"

# Windows PowerShell
$env:LLM__ApiKey = "sk-your-api-key-here"
$env:LLM__Model = "gpt-4-turbo-preview"
```

#### Option 3: User Secrets (Recommended for Development)

```bash
cd src/DNDGame.API
dotnet user-secrets init
dotnet user-secrets set "LLM:ApiKey" "sk-your-api-key-here"
dotnet user-secrets set "LLM:Model" "gpt-4-turbo-preview"
```

### Production Environment

**Never commit API keys to version control!**

Use:
- **Azure Key Vault** for Azure deployments
- **AWS Secrets Manager** for AWS deployments
- **Docker secrets** for containerized deployments
- **Environment variables** injected by orchestrator

```bash
# Docker Compose example
docker run -d \
  -e LLM__ApiKey=${OPENAI_API_KEY} \
  -e LLM__Model=gpt-4-turbo-preview \
  dndgame-api
```

---

## API Endpoints

### 1. Generate DM Response

**POST** `/api/v1/dm/generate`

Generates a narrative response to player action with full context.

#### Request
```json
{
  "sessionId": 1,
  "playerAction": "I search the ancient library for clues about the artifact",
  "contextMessages": 10
}
```

#### Response (200 OK)
```json
{
  "content": "As you run your fingers along the dusty spines, a leather-bound tome catches your eye. The pages crackle as you open it, revealing cryptic runes that seem to glow faintly in the dim light. Make an Investigation check.",
  "suggestedActions": [
    "Roll Investigation (Intelligence)",
    "Examine the runes more closely",
    "Search for more books",
    "Call for your companions"
  ],
  "tokensUsed": 45,
  "responseTimeMs": 850,
  "estimatedCost": 0.00135,
  "wasModerated": false
}
```

#### Errors
- **400 Bad Request**: Validation failure or content blocked by moderation
- **404 Not Found**: Session does not exist
- **500 Internal Server Error**: LLM API failure (after retries)

---

### 2. Stream DM Response (SSE)

**POST** `/api/v1/dm/stream`

Streams response incrementally using Server-Sent Events.

#### Request
```json
{
  "sessionId": 1,
  "playerAction": "I cast Fireball at the goblin horde",
  "contextMessages": 10
}
```

#### Response (200 OK, text/event-stream)
```
data: The 
data: flames 
data: erupt 
data: from 
data: your 
data: fingertips, 
data: engulfing 
data: the 
data: goblins 
data: in 
data: a 
data: blazing 
data: inferno...
[DONE]
```

#### Client Usage (JavaScript)
```javascript
const response = await fetch('/api/v1/dm/stream', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    sessionId: 1,
    playerAction: 'I attack the dragon'
  })
});

const reader = response.body.getReader();
const decoder = new TextDecoder();

while (true) {
  const { value, done } = await reader.read();
  if (done) break;
  
  const chunk = decoder.decode(value);
  const lines = chunk.split('\n');
  
  for (const line of lines) {
    if (line.startsWith('data: ')) {
      const text = line.substring(6);
      if (text === '[DONE]') {
        console.log('Stream complete');
      } else {
        console.log('Token:', text);
      }
    }
  }
}
```

---

### 3. Generate NPC Dialogue

**POST** `/api/v1/dm/npc`

Generates dialogue for an NPC with personality context.

#### Request
```json
{
  "sessionId": 1,
  "npcName": "Elara the Merchant",
  "npcPersonality": "Shrewd, talkative, loves gossip",
  "npcOccupation": "Merchant",
  "playerMessage": "Do you know anything about the old castle?",
  "contextMessages": 5
}
```

#### Response (200 OK)
```json
{
  "content": "Ah, the old castle! *leans in conspiratorially* I've heard whispers, friend. Dark whispers. They say the lord who once lived there... well, let's just say he had peculiar tastes. Nobody who ventured there has returned in twenty years. But for the right price, I could tell you more...",
  "suggestedActions": [
    "Offer gold for information",
    "Press for details",
    "Thank her and leave",
    "Ask about the castle's history"
  ],
  "tokensUsed": 52,
  "responseTimeMs": 920,
  "estimatedCost": 0.00156,
  "wasModerated": false
}
```

---

### 4. Generate Scene Description

**POST** `/api/v1/dm/scene`

Generates a detailed description of a location.

#### Request
```json
{
  "sessionId": 1,
  "locationName": "The Prancing Pony",
  "locationType": "Tavern",
  "locationDescription": "A cozy inn in the village of Bree",
  "features": ["Roaring fireplace", "Oak bar", "Private rooms upstairs"],
  "npcsPresent": ["Barliman Butterbur (innkeeper)", "Hooded stranger"],
  "contextMessages": 3
}
```

#### Response (200 OK)
```json
{
  "content": "You push open the heavy oak door and step into the warm embrace of The Prancing Pony. A roaring fireplace crackles in the corner, casting dancing shadows across the worn wooden tables. The air is thick with the scent of pipe smoke and roasted meat. Behind the bar, Barliman Butterbur wipes down mugs, his round face breaking into a smile as he sees you. In the far corner, a hooded figure hunches over a tankard, face obscured by shadows.",
  "suggestedActions": [
    "Approach the bar",
    "Investigate the hooded stranger",
    "Find a table",
    "Ask about rooms"
  ],
  "tokensUsed": 68,
  "responseTimeMs": 1050,
  "estimatedCost": 0.00204,
  "wasModerated": false
}
```

---

## Prompt Customization

### System Prompts

Located in `PromptTemplateService.cs`, these define the DM's personality and behavior.

#### Base System Prompt

```csharp
private const string BaseSystemPrompt = """
You are an expert Dungeon Master running a Dungeons & Dragons 5th Edition campaign.
Your role is to:
- Create immersive, engaging narratives
- Respond to player actions with creativity and fairness
- Follow D&D 5e rules accurately
- Maintain consistency with established facts
- Describe scenes vividly using all five senses
- Create memorable NPCs with distinct personalities
- Balance challenge and fun
- Never contradict previously established information

Keep responses concise (2-4 paragraphs).
End with a clear prompt for player action.
""";
```

#### Customization

To modify the DM's style, edit `GetSystemPrompt()`:

```csharp
public string GetSystemPrompt(SessionMode mode)
{
    var modeContext = mode switch
    {
        SessionMode.Solo => "This is a solo adventure. Focus on the single player's journey.",
        SessionMode.Multiplayer => "This is a party adventure. Balance spotlight between players.",
        _ => ""
    };
    
    // Add your custom instructions here
    var customTone = "Use a dramatic, epic tone reminiscent of Lord of the Rings.";
    
    return $"{BaseSystemPrompt}\n\n{modeContext}\n\n{customTone}";
}
```

---

### Combat Prompts

```csharp
private const string CombatPromptTemplate = """
=== COMBAT ===
The party is engaged in combat. Characters:
{0}

Focus on:
- Tactical descriptions (positioning, cover, range)
- Consequences of actions (damage, status effects)
- Dynamic environment changes
- Enemy reactions and strategies
- Clear indication when to roll dice

Keep the action fast-paced and exciting.
""";
```

**Customization Example**: Add critical hit descriptions

```csharp
var combatPrompt = GetCombatPrompt();
combatPrompt += "\n\nWhen a critical hit occurs, describe it with extra flair!";
```

---

### Exploration Prompts

```csharp
private const string ExplorationPromptTemplate = """
=== EXPLORATION ===
The party is exploring. Current location: {0}

Focus on:
- Sensory details (sights, sounds, smells, textures, tastes)
- Hidden details to discover
- Environmental storytelling
- Potential dangers or treasures
- Opportunities for player creativity

Encourage investigation and interaction.
""";
```

**Customization Example**: Emphasize mystery

```csharp
var explorationPrompt = GetExplorationPrompt();
explorationPrompt += "\n\nForeshadow future events with subtle clues.";
```

---

## Content Moderation

### Built-In Keywords

The system filters inappropriate content using keyword lists:

#### NSFW Keywords
```csharp
private static readonly HashSet<string> _nsfwKeywords = new(StringComparer.OrdinalIgnoreCase)
{
    "nsfw", "explicit", "sexual", "nude", "naked", "porn"
};
```

#### Harassment Keywords
```csharp
private static readonly HashSet<string> _harassmentKeywords = new(StringComparer.OrdinalIgnoreCase)
{
    "kill yourself", "kys", "hate", "racist", "slur", "violence", "abuse"
};
```

### Custom Moderation

To add custom filters, edit `ContentModerationService.cs`:

```csharp
private static readonly HashSet<string> _customKeywords = new(StringComparer.OrdinalIgnoreCase)
{
    "your_custom_word_1",
    "your_custom_word_2",
    "your_custom_phrase"
};

public async Task<ModerationResult> ModerateInputAsync(string content, CancellationToken ct = default)
{
    // ... existing checks
    
    // Add custom check
    if (_customKeywords.Any(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
    {
        return ModerationResult.Unsafe(new List<string> { "Custom policy violation" });
    }
    
    return ModerationResult.Safe();
}
```

### Configuration Options

```json
{
  "ContentModeration": {
    "Enabled": true,              // Master switch
    "BlockNSFW": true,             // Filter sexual content
    "BlockHarassment": true,       // Filter harassment
    "MaxInputLength": 2000,        // Character limit
    "SanitizeOutput": true,        // Auto-sanitize LLM responses
    "ReplacementText": "[REDACTED]" // Text for sanitized content
  }
}
```

### Disabling Moderation (Development Only)

```json
{
  "ContentModeration": {
    "Enabled": false
  }
}
```

**⚠️ Never disable in production!**

---

## Streaming Responses

### Server-Side Implementation

Streaming uses `IAsyncEnumerable<string>`:

```csharp
public async IAsyncEnumerable<string> StreamResponseAsync(
    SessionContext context,
    string playerAction,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var systemPrompt = GetSystemPrompt(context);
    var userMessage = FormatContext(context, playerAction);
    
    await foreach (var token in _llmProvider.StreamCompleteAsync(systemPrompt, userMessage, cancellationToken))
    {
        yield return token;
    }
}
```

### Client-Side Consumption (Blazor)

```csharp
@code {
    private string dmResponse = "";
    
    private async Task StreamDmResponseAsync()
    {
        dmResponse = "";
        
        var response = await Http.PostAsJsonAsync("/api/v1/dm/stream", new
        {
            SessionId = 1,
            PlayerAction = playerAction
        });
        
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line?.StartsWith("data: ") == true)
            {
                var token = line[6..]; // Remove "data: " prefix
                
                if (token == "[DONE]")
                    break;
                
                dmResponse += token;
                StateHasChanged(); // Update UI
                await Task.Delay(10); // Smooth rendering
            }
        }
    }
}
```

---

## Testing

### Unit Tests with Mocked LLM

```csharp
public class LlmDmServiceTests
{
    private readonly Mock<ILlmProvider> _mockLlmProvider;
    private readonly LlmDmService _sut;
    
    public LlmDmServiceTests()
    {
        _mockLlmProvider = new Mock<ILlmProvider>();
        _sut = new LlmDmService(
            _mockLlmProvider.Object,
            /* other dependencies */
        );
    }
    
    [Fact]
    public async Task GenerateResponseAsync_WithSafeInput_ReturnsResponse()
    {
        // Arrange
        _mockLlmProvider
            .Setup(p => p.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("The dragon roars at you!", 25));
        
        var context = CreateTestContext();
        
        // Act
        var result = await _sut.GenerateResponseAsync(context, "I approach the dragon");
        
        // Assert
        result.Content.Should().Be("The dragon roars at you!");
        result.TokensUsed.Should().Be(25);
    }
}
```

### Integration Tests with Real Database

```csharp
public class DmControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    
    [Fact]
    public async Task GenerateDmResponse_WithValidSession_ReturnsSuccess()
    {
        // Arrange
        var session = await CreateTestSessionAsync();
        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerAction = "I search the room",
            ContextMessages = 5
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        result.Should().NotBeNull();
        result!.Content.Should().NotBeEmpty();
        result.TokensUsed.Should().BeGreaterThan(0);
        result.ResponseTimeMs.Should().BeGreaterThan(0);
    }
}
```

---

## Cost Management

### Token Pricing (OpenAI GPT-4 Turbo)

| Model | Input | Output |
|-------|-------|--------|
| `gpt-4-turbo-preview` | $0.01/1K tokens | $0.03/1K tokens |
| `gpt-3.5-turbo` | $0.0005/1K tokens | $0.0015/1K tokens |

### Cost Estimation

The API returns `estimatedCost` based on token usage:

```csharp
public decimal EstimatedCost => TokensUsed * 0.00003m; // $0.03/1K tokens
```

**Example**:
- 45 tokens × $0.00003 = $0.00135 per response
- 100 responses = $0.135
- 1000 responses = $1.35

### Cost Optimization Strategies

#### 1. Use Smaller Models for Simple Tasks

```json
{
  "LLM": {
    "Model": "gpt-3.5-turbo"  // 20x cheaper than GPT-4
  }
}
```

#### 2. Reduce MaxTokens

```json
{
  "LLM": {
    "MaxTokens": 300  // Down from 500
  }
}
```

#### 3. Limit Context Messages

```csharp
var request = new GenerateDmResponseRequest
{
    SessionId = 1,
    PlayerAction = "I attack",
    ContextMessages = 5  // Down from 10
};
```

#### 4. Implement Caching

Cache responses for repeated queries:

```csharp
public class CachedLlmProvider : ILlmProvider
{
    private readonly IMemoryCache _cache;
    private readonly ILlmProvider _inner;
    
    public async Task<(string Content, int TokensUsed)> CompleteAsync(
        string systemPrompt, string userMessage, CancellationToken ct = default)
    {
        var cacheKey = $"{systemPrompt}:{userMessage}";
        
        if (_cache.TryGetValue<(string, int)>(cacheKey, out var cached))
            return cached;
        
        var result = await _inner.CompleteAsync(systemPrompt, userMessage, ct);
        
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        
        return result;
    }
}
```

#### 5. Monitor Usage

Track costs in real-time:

```csharp
public class CostTrackingService
{
    private decimal _totalCost = 0;
    
    public void TrackResponse(DmResponse response)
    {
        _totalCost += response.EstimatedCost;
        
        if (_totalCost > 10.0m) // $10 threshold
        {
            // Send alert
            _logger.LogWarning("Daily LLM cost exceeded $10: {Cost}", _totalCost);
        }
    }
}
```

---

## Troubleshooting

### Common Issues

#### 1. "401 Unauthorized" from OpenAI API

**Cause**: Invalid or missing API key

**Solution**:
```bash
# Verify key is set
dotnet user-secrets list

# Set key
dotnet user-secrets set "LLM:ApiKey" "sk-your-key"
```

#### 2. "429 Too Many Requests"

**Cause**: Rate limit exceeded

**Solution**: Polly retry policy handles this automatically with exponential backoff:

```csharp
_retryPolicy = Policy
    .Handle<ClientResultException>(ex => ex.Status == 429 || ex.Status >= 500)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
```

If persistent, upgrade to a higher rate limit tier.

#### 3. "Content moderation blocked request"

**Cause**: Input contained blocked keywords

**Solution**:
- Review `ContentModerationSettings`
- Check player input for inappropriate content
- Disable moderation temporarily for debugging:

```json
{
  "ContentModeration": {
    "Enabled": false
  }
}
```

#### 4. Slow Response Times (>5s)

**Causes**:
- Large `MaxTokens` setting
- Too many `ContextMessages`
- Using GPT-4 instead of GPT-3.5

**Solutions**:
```json
{
  "LLM": {
    "Model": "gpt-3.5-turbo",  // Faster model
    "MaxTokens": 300            // Reduce tokens
  }
}
```

```csharp
var request = new GenerateDmResponseRequest
{
    ContextMessages = 5  // Reduce context
};
```

#### 5. Empty or Incomplete Responses

**Cause**: `MaxTokens` too low for response

**Solution**:
```json
{
  "LLM": {
    "MaxTokens": 500  // Increase limit
  }
}
```

#### 6. Streaming Not Working

**Cause**: Client not handling SSE format correctly

**Solution**: Ensure `Accept: text/event-stream` header:

```javascript
fetch('/api/v1/dm/stream', {
  headers: {
    'Accept': 'text/event-stream',
    'Content-Type': 'application/json'
  }
})
```

---

## Best Practices

### 1. Always Use Moderation in Production

```json
{
  "ContentModeration": {
    "Enabled": true,
    "BlockNSFW": true,
    "BlockHarassment": true
  }
}
```

### 2. Set Budget Alerts

Configure OpenAI dashboard:
- **Hard limit**: $50/month
- **Soft limit**: $25/month (email alert)

### 3. Log Token Usage

```csharp
_logger.LogInformation(
    "LLM request completed. Model: {Model}, Tokens: {Tokens}, Cost: ${Cost:F4}, Time: {Time}ms",
    settings.Model,
    response.TokensUsed,
    response.EstimatedCost,
    response.ResponseTimeMs
);
```

### 4. Use Streaming for Long Responses

Improves perceived performance:
```csharp
// Instead of waiting 5 seconds
var response = await _dmService.GenerateResponseAsync(context, action);

// Stream incrementally (appears faster)
await foreach (var token in _dmService.StreamResponseAsync(context, action))
{
    Console.Write(token);
}
```

### 5. Handle Cancellation

Allow users to cancel long-running requests:

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10s timeout

try
{
    var response = await _dmService.GenerateResponseAsync(context, action, cts.Token);
}
catch (OperationCanceledException)
{
    return BadRequest("Request timed out");
}
```

---

## Additional Resources

- [OpenAI API Documentation](https://platform.openai.com/docs/api-reference)
- [OpenAI .NET SDK](https://github.com/openai/openai-dotnet)
- [Polly Retry Policies](https://github.com/App-vNext/Polly)
- [Server-Sent Events Specification](https://html.spec.whatwg.org/multipage/server-sent-events.html)
- [ASP.NET Core Options Pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)

---

**Need help?** Open an issue on [GitHub](https://github.com/SDCHESNEY/DNDGame-DotNet/issues).
