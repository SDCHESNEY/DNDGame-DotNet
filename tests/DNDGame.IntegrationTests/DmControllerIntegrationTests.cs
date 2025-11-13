using System.Net;
using System.Net.Http.Json;
using System.Text;
using DNDGame.Application.DTOs;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Models;
using DNDGame.Core.ValueObjects;
using DNDGame.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DNDGame.IntegrationTests;

/// <summary>
/// Integration tests for DM Controller endpoints with real API and database interactions.
/// </summary>
public class DmControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DmControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateDmResponse_WithValidSession_ReturnsSuccess()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        _factory.SetupMockLlmResponse("You explore the ancient ruins. What do you do next?", 45);

        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I search the room for traps",
            CharacterId = session.Participants.First().CharacterId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        dmResponse!.Content.Should().Contain("explore");
        dmResponse.TokensUsed.Should().Be(45);
        dmResponse.ResponseTimeMs.Should().BeGreaterThan(0);
        dmResponse.EstimatedCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateDmResponse_WithInvalidSession_ReturnsNotFound()
    {
        // Arrange
        var request = new GenerateDmResponseRequest
        {
            SessionId = 99999,
            PlayerMessage = "test message"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GenerateDmResponse_WithBlockedContent_ReturnsBadRequest()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        
        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "explicit nsfw content here"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateDmResponse_InCombat_UsesCombatPrompts()
    {
        // Arrange
        var session = await CreateTestSessionInCombat();
        _factory.SetupMockLlmResponse("The goblin attacks! Roll initiative.", 35);

        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I attack with my sword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        dmResponse!.Content.Should().NotBeEmpty();
        
        // Verify the LLM provider was called (combat scenario)
        _factory.MockLlmProvider.Verify(
            p => p.CompleteAsync(
                It.Is<string>(s => s.Contains("Dungeon Master")),
                It.Is<string>(s => s.Contains("sword")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateDmResponse_MultiTurnConversation_MaintainsContext()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        
        // Turn 1
        _factory.SetupMockLlmResponse("You enter a dark tavern.", 30);
        var request1 = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I enter the tavern"
        };
        await _client.PostAsJsonAsync("/api/v1/dm/generate", request1);

        // Save turn 1 message
        await AddMessageToSession(session.Id, "Player", "I enter the tavern");
        await AddMessageToSession(session.Id, "DM", "You enter a dark tavern.");

        // Turn 2
        _factory.SetupMockLlmResponse("The bartender looks at you suspiciously.", 35);
        var request2 = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I approach the bartender"
        };

        // Act
        var response2 = await _client.PostAsJsonAsync("/api/v1/dm/generate", request2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response2.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        dmResponse!.Content.Should().Contain("bartender");
    }

    [Fact]
    public async Task StreamDmResponse_ShouldStreamChunks()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        _factory.SetupMockLlmStream("You ", "enter ", "the ", "dark ", "forest.");

        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I explore the forest",
            Stream = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/stream", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/event-stream");

        var streamContent = await response.Content.ReadAsStringAsync();
        streamContent.Should().Contain("data: You");
        streamContent.Should().Contain("data: enter");
        streamContent.Should().Contain("data: [DONE]");
    }

    [Fact]
    public async Task GenerateNpcDialogue_WithValidNpc_ReturnsDialogue()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        _factory.SetupMockLlmResponse("Greetings, traveler! Welcome to my humble shop.", 40);

        var request = new GenerateNpcDialogueRequest
        {
            SessionId = session.Id,
            NpcName = "Elara the Merchant",
            Personality = "Friendly and talkative",
            Occupation = "Shop Owner",
            Mood = "Cheerful",
            PlayerMessage = "Hello, what do you have for sale?"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/npc", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        dmResponse!.Content.Should().Contain("Greetings");
        dmResponse.TokensUsed.Should().Be(40);
    }

    [Fact]
    public async Task GenerateSceneDescription_WithLocation_ReturnsDescription()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        _factory.SetupMockLlmResponse("The tavern is dimly lit with flickering candles. Patrons whisper in dark corners.", 55);

        var request = new GenerateSceneDescriptionRequest
        {
            SessionId = session.Id,
            LocationName = "The Prancing Pony",
            LocationType = "Tavern",
            Description = "A cozy roadside inn",
            Features = new List<string> { "Bar", "Fireplace", "Private rooms" },
            NpcsPresent = new List<string> { "Bartender", "Mysterious hooded figure" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/scene", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        dmResponse!.Content.Should().Contain("tavern");
        dmResponse.TokensUsed.Should().Be(55);
    }

    [Fact]
    public async Task GenerateDmResponse_WithSanitizedOutput_ReturnsCleanContent()
    {
        // Arrange
        var session = await CreateTestSessionWithCharacter();
        _factory.SetupMockLlmResponse("You find explicit content in the chest.", 30);

        var request = new GenerateDmResponseRequest
        {
            SessionId = session.Id,
            PlayerMessage = "I open the chest"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/dm/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dmResponse = await response.Content.ReadFromJsonAsync<DmResponseDto>();
        dmResponse.Should().NotBeNull();
        // Content should be sanitized if it contained blocked words
        dmResponse!.WasModerated.Should().BeTrue();
        dmResponse.Content.Should().Contain("[REDACTED]");
        dmResponse.Content.Should().NotContain("explicit");
    }

    // Helper Methods

    private async Task<Session> CreateTestSessionWithCharacter()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DndGameContext>();

        var player = new Player
        {
            UserId = $"test-user-{Guid.NewGuid()}",
            DisplayName = $"TestPlayer_{Guid.NewGuid()}",
            CreatedAt = DateTime.UtcNow
        };

        var character = new Character
        {
            Name = "Test Hero",
            Class = CharacterClass.Fighter,
            Level = 5,
            HitPoints = 40,
            MaxHitPoints = 50,
            ArmorClass = 16,
            AbilityScores = new AbilityScores(16, 14, 16, 10, 12, 10),
            Player = player,
            CreatedAt = DateTime.UtcNow
        };

        var session = new Session
        {
            Title = "Test Adventure",
            Mode = SessionMode.Solo,
            State = SessionState.Created,
            CurrentScene = "Test Location",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        var participant = new SessionParticipant
        {
            Session = session,
            CharacterId = character.Id,
            Character = character,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        db.Players.Add(player);
        db.Characters.Add(character);
        db.Sessions.Add(session);
        db.SessionParticipants.Add(participant);
        await db.SaveChangesAsync();

        return session;
    }

    private async Task<Session> CreateTestSessionInCombat()
    {
        var session = await CreateTestSessionWithCharacter();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DndGameContext>();
        
        var existingSession = await db.Sessions.FindAsync(session.Id);
        if (existingSession != null)
        {
            existingSession.State = SessionState.InProgress;
            await db.SaveChangesAsync();
        }

        return existingSession ?? session;
    }

    private async Task AddMessageToSession(int sessionId, string role, string content)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DndGameContext>();

        var message = new Message
        {
            SessionId = sessionId,
            AuthorId = role == "DM" ? "0" : "1",
            Role = role == "DM" ? MessageRole.DungeonMaster : MessageRole.Player,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();
    }
}
