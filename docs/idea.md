# LLM Dungeon Master RPG — .NET Core, Blazor & MAUI Hybrid Architecture

A cooperative, text-first, Dungeons & Dragons–style role‑playing game where a Large Language Model (LLM) plays the Dungeon Master (DM). Players interact via a **Blazor web UI** and **.NET MAUI mobile apps**, playing solo or together in real time. Characters can be chosen from a curated portfolio of D&D‑style archetypes or created from scratch.

> **Trademark note**: "Dungeons & Dragons" is property of Wizards of the Coast. This project is a fan‑made, non‑commercial homage. Use only content permitted by the SRD 5.1 or original material; do not distribute proprietary D&D content.

## Vision

- Bring the magic of a great DM to anyone, on demand, with collaborative storytelling and fair rules.
- Seamless single‑player and multiplayer experiences, sharing one consistent world state.
- Extensible rules modules (SRD‑compatible) with pluggable dice mechanics and content safety.
- **Cross-platform support**: Blazor WebAssembly/Server for web, .NET MAUI for iOS/Android/Windows/macOS.

## Features at a Glance

- **LLM‑powered DM** that plans scenes, runs encounters, role‑plays NPCs, and adjudicates outcomes.
- **Three frontends**: 
  - Blazor WebAssembly for responsive web experience
  - Blazor Server for real-time web gameplay
  - .NET MAUI Blazor Hybrid for native mobile apps
- **Single‑player campaign** or **multiplayer party** with turn/initiative support.
- **Character portfolio** with common fantasy archetypes (Fighter, Wizard, Rogue, Cleric, etc.) and custom builders.
- **Persistent worlds**, sessions, and character progress via Entity Framework Core.

## Game Modes

- **Single‑player**: 1 player + LLM DM in a private session.
- **Multiplayer**: 2–6 players + LLM DM in a shared room with synchronized state via SignalR.

## Core Roles

- **Dungeon Master (LLM)**: narrative control, scene framing, NPCs, rules adjudication, loot, and world state updates.
- **Players (humans)**: choose actions in narrative and combat; interact via UI; roll dice (client or server).

## Gameplay Loop

1. **Session start**: LLM summarizes the prior state and hooks the next scene.
2. **Exploration/role‑play**: freeform choices; DM adapts.
3. **Challenges/encounters**: initiative, turns, checks/saves, resource tracking.
4. **Resolution and rewards**: XP, loot, downtime.
5. **Save state**, next session recap prompt.

---

## LLM Dungeon Master Design

### Goals
- Be collaborative, fair, and SRD‑aligned. Avoid railroading; present meaningful choices.
- Maintain a canonical world state; never contradict prior facts without an in‑world reason.
- Keep responses concise in combat, more descriptive in exploration; always end with clear choices.

### System Prompt Skeleton

- **Campaign frame**: genre, tone, safety/content boundaries.
- **Rules mode**: SRD‑compatible checks, DCs, and conditions; never reveal internal DCs ahead.
- **Turn loop**: in combat, ask for one character action; in exploration, propose 2–4 plausible options and accept freeform input.
- **Dice policy**: either server‑rolled or client‑rolled with verification; explain outcomes.
- **State discipline**: append to session log with timestamped entries; include NPC ledger, quest log, and world flags.

### Guardrails & Safety
- Content moderation for inputs/outputs (violence/NSFW filters, harassment safeguards).
- Refusal policies: politely reframe disallowed content.
- Prompt hardening: role/goal reminders, tool usage hints, and memory summaries.

### Rules & Dice Integration
- Ability checks, skill checks, saving throws, attack/AC, damage, conditions.
- Configurable: advantage/disadvantage, passive perception, optional SRD modules.
- Server dice: cryptographically secure RNG via `RandomNumberGenerator`; client dice: require declaration and echo results.

### Memory/State
- **Session memory**: scene history, inventory, HP, spell slots, conditions.
- **Global campaign memory**: world map flags, factions, recurring NPCs, arcs.
- Summarize long histories; keep retrieval‑ready chunks for the LLM via vector database (optional).

---

## Character Portfolio

- **Templates**: Fighter, Wizard, Rogue, Cleric, Ranger, Bard, Paladin, Warlock, Monk, Druid (SRD‑compatible archetypes). Each template defines base stats, proficiencies, starting equipment, and 1–2 subclass‑agnostic features.
- **Custom builder**: point‑buy or standard array, background, skills, starting gear. Enforce validation via FluentValidation.
- **Import/export**: JSON character sheets with System.Text.Json serialization.
- **Personas**: optional personality tags (e.g., "Stoic Veteran", "Curious Scholar") to flavor role‑play.

---

## High‑Level Architecture

### Technology Stack

- **Backend**: .NET 8/9 Core Web API (ASP.NET Core)
- **Real-time Communication**: SignalR for WebSocket-based multiplayer
- **Web Frontend**: 
  - **Blazor Server** for real-time gameplay with low latency
  - **Blazor WebAssembly** for offline-capable web client
- **Mobile Frontend**: **.NET MAUI Blazor Hybrid** for iOS, Android, Windows, macOS
- **Persistence**: 
  - **Entity Framework Core** with SQL Server (production) / SQLite (development)
  - **Azure Cache for Redis** (optional) for session state and pub/sub
- **LLM Integration**: 
  - **Semantic Kernel** or direct HTTP clients for OpenAI, Azure OpenAI, Anthropic
  - Rate limiting via AspNetCoreRateLimit
  - Retry policies with Polly
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Testing**: xUnit, FluentAssertions, Moq, bUnit (Blazor component tests)

### Component Responsibilities

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Applications                      │
├─────────────────┬─────────────────┬────────────────────────┤
│  Blazor Server  │ Blazor WASM     │  .NET MAUI Hybrid      │
│  (Web - SSR)    │ (Web - Client)  │  (iOS/Android/Desktop) │
└────────┬────────┴────────┬────────┴──────────┬─────────────┘
         │                 │                    │
         └─────────────────┼────────────────────┘
                           │
                   ┌───────▼────────┐
                   │   API Gateway  │
                   │  (ASP.NET Core)│
                   └───────┬────────┘
                           │
         ┌─────────────────┼─────────────────┐
         │                 │                  │
    ┌────▼────┐      ┌────▼─────┐      ┌───▼────┐
    │ SignalR │      │   REST   │      │  Auth  │
    │  Hubs   │      │   APIs   │      │ (JWT)  │
    └────┬────┘      └────┬─────┘      └───┬────┘
         │                │                 │
         └────────────────┼─────────────────┘
                          │
              ┌───────────▼────────────┐
              │    Service Layer       │
              ├────────────────────────┤
              │ - SessionService       │
              │ - CharacterService     │
              │ - DiceRollerService    │
              │ - LlmDmService         │
              │ - RulesEngineService   │
              └───────────┬────────────┘
                          │
              ┌───────────▼────────────┐
              │   Data Access Layer    │
              ├────────────────────────┤
              │ - EF Core DbContext    │
              │ - Repositories         │
              │ - Unit of Work         │
              └───────────┬────────────┘
                          │
              ┌───────────▼────────────┐
              │      Database          │
              │  SQL Server / SQLite   │
              └────────────────────────┘
```

### Project Structure

```
DNDGame.sln
│
├── src/
│   ├── DNDGame.Core/                    # Domain models, interfaces
│   │   ├── Entities/
│   │   │   ├── Character.cs
│   │   │   ├── Session.cs
│   │   │   ├── Player.cs
│   │   │   ├── Message.cs
│   │   │   └── DiceRoll.cs
│   │   ├── Interfaces/
│   │   │   ├── ICharacterService.cs
│   │   │   ├── ISessionService.cs
│   │   │   ├── IDiceRoller.cs
│   │   │   └── ILlmDmService.cs
│   │   └── ValueObjects/
│   │       ├── AbilityScores.cs
│   │       ├── DiceFormula.cs
│   │       └── CharacterClass.cs
│   │
│   ├── DNDGame.Infrastructure/          # Data access, external services
│   │   ├── Data/
│   │   │   ├── DndGameContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── CharacterRepository.cs
│   │   │   └── SessionRepository.cs
│   │   └── Services/
│   │       ├── LlmDmService.cs
│   │       └── DiceRollerService.cs
│   │
│   ├── DNDGame.Application/             # Business logic
│   │   ├── Services/
│   │   │   ├── CharacterService.cs
│   │   │   ├── SessionService.cs
│   │   │   └── RulesEngineService.cs
│   │   ├── DTOs/
│   │   │   ├── CreateCharacterRequest.cs
│   │   │   ├── CreateSessionRequest.cs
│   │   │   └── DiceRollResult.cs
│   │   └── Validators/
│   │       ├── CreateCharacterValidator.cs
│   │       └── CreateSessionValidator.cs
│   │
│   ├── DNDGame.API/                     # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── CharactersController.cs
│   │   │   ├── SessionsController.cs
│   │   │   └── PlayersController.cs
│   │   ├── Hubs/
│   │   │   └── GameSessionHub.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── DNDGame.Web.Server/              # Blazor Server app
│   │   ├── Components/
│   │   │   ├── Pages/
│   │   │   ├── Layout/
│   │   │   └── Shared/
│   │   ├── Services/
│   │   └── Program.cs
│   │
│   ├── DNDGame.Web.Client/              # Blazor WebAssembly app
│   │   ├── Components/
│   │   ├── Services/
│   │   └── Program.cs
│   │
│   ├── DNDGame.Shared/                  # Shared Blazor components
│   │   ├── Components/
│   │   │   ├── CharacterSheet.razor
│   │   │   ├── DiceRoller.razor
│   │   │   ├── ChatPanel.razor
│   │   │   └── InitiativeTracker.razor
│   │   └── Models/
│   │
│   └── DNDGame.MauiApp/                 # .NET MAUI Blazor Hybrid
│       ├── Platforms/
│       │   ├── Android/
│       │   ├── iOS/
│       │   ├── Windows/
│       │   └── MacCatalyst/
│       ├── Resources/
│       ├── Pages/
│       ├── ViewModels/
│       ├── Services/
│       ├── MauiProgram.cs
│       └── App.xaml
│
└── tests/
    ├── DNDGame.UnitTests/
    ├── DNDGame.IntegrationTests/
    └── DNDGame.BlazorTests/
```

---

## Domain Model (Detailed)

```csharp
// Core Entities

public class Player
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Character> Characters { get; set; } = [];
}

public class Character
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public required string Name { get; set; }
    public CharacterClass Class { get; set; }
    public int Level { get; set; }
    public AbilityScores AbilityScores { get; set; } = null!;
    public int HitPoints { get; set; }
    public int MaxHitPoints { get; set; }
    public int ArmorClass { get; set; }
    public List<string> Skills { get; set; } = [];
    public List<InventoryItem> Inventory { get; set; } = [];
    public string? PersonalityTraits { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Session
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public SessionMode Mode { get; set; }
    public SessionState State { get; set; }
    public string? CurrentScene { get; set; }
    public int? CurrentTurnCharacterId { get; set; }
    public List<SessionParticipant> Participants { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
    public List<DiceRoll> DiceRolls { get; set; } = [];
    public Dictionary<string, object> WorldFlags { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class Message
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public required string AuthorId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DiceRoll
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public required string RollerId { get; set; }
    public required string Formula { get; set; }
    public int[] IndividualRolls { get; set; } = [];
    public int Modifier { get; set; }
    public int Total { get; set; }
    public DiceRollType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

// Enums

public enum CharacterClass
{
    Fighter, Wizard, Rogue, Cleric, Ranger, 
    Bard, Paladin, Warlock, Monk, Druid
}

public enum SessionMode
{
    Solo,
    Multiplayer
}

public enum SessionState
{
    Created,
    WaitingForPlayers,
    InProgress,
    Paused,
    Completed,
    Abandoned
}

public enum MessageRole
{
    DungeonMaster,
    Player,
    System
}

public enum DiceRollType
{
    AbilityCheck,
    SavingThrow,
    Attack,
    Damage,
    Initiative,
    Custom
}

// Value Objects

public record AbilityScores(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma)
{
    public int GetModifier(int score) => (score - 10) / 2;
}
```

---

## Blazor Web UI Architecture

### Blazor Server vs WebAssembly

**Blazor Server** (Primary for gameplay):
- Real-time updates via SignalR connection
- Low latency for DM responses
- Server-side rendering with circuit management
- Ideal for active gameplay sessions

**Blazor WebAssembly** (Secondary for character management):
- Offline-capable PWA for character creation
- Better for character portfolio browsing
- Can sync when reconnected
- Reduced server load

### UI Layout

```razor
@* Pages/Session.razor *@
@page "/session/{SessionId:int}"
@inject ISessionService SessionService
@inject NavigationManager Navigation
@implements IAsyncDisposable

<div class="session-container">
    <div class="session-header">
        <h2>@session?.Title</h2>
        <SessionStatus State="@session?.State" />
    </div>
    
    <div class="main-content">
        <!-- Left Panel: Chat & Actions -->
        <div class="chat-panel">
            <ChatPanel Messages="@messages" 
                       OnMessageSent="SendMessageAsync" />
            <DiceRoller OnRollDice="HandleDiceRollAsync" />
        </div>
        
        <!-- Right Panel: Character & Party Info -->
        <div class="info-panel">
            <TabControl>
                <TabPage Title="Character">
                    <CharacterSheet Character="@currentCharacter" />
                </TabPage>
                <TabPage Title="Party">
                    <PartyPanel Participants="@session?.Participants" />
                </TabPage>
                <TabPage Title="Initiative">
                    <InitiativeTracker CurrentTurn="@session?.CurrentTurnCharacterId" />
                </TabPage>
                <TabPage Title="Inventory">
                    <InventoryPanel Items="@currentCharacter?.Inventory" />
                </TabPage>
            </TabControl>
        </div>
    </div>
</div>

@code {
    [Parameter]
    public int SessionId { get; set; }
    
    private Session? session;
    private Character? currentCharacter;
    private List<Message> messages = [];
    private HubConnection? hubConnection;
    
    protected override async Task OnInitializedAsync()
    {
        session = await SessionService.GetSessionAsync(SessionId);
        await ConnectToSignalRAsync();
    }
    
    private async Task ConnectToSignalRAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/hubs/game-session"))
            .WithAutomaticReconnect()
            .Build();
        
        hubConnection.On<Message>("ReceiveMessage", message =>
        {
            messages.Add(message);
            StateHasChanged();
        });
        
        hubConnection.On<DiceRollResult>("DiceRolled", result =>
        {
            // Handle dice roll result
            StateHasChanged();
        });
        
        await hubConnection.StartAsync();
        await hubConnection.InvokeAsync("JoinSession", SessionId);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

### Key Blazor Components

1. **CharacterSheet.razor**: Display and edit character stats, abilities, skills
2. **DiceRoller.razor**: Interactive dice roller with formula input (2d20+5)
3. **ChatPanel.razor**: Real-time message display with DM/player differentiation
4. **InitiativeTracker.razor**: Combat turn order with current turn highlighting
5. **InventoryPanel.razor**: Character equipment and items
6. **SpellSlots.razor**: Track and manage spell slots for casters
7. **ConditionsPanel.razor**: Active conditions (stunned, prone, etc.)

---

## .NET MAUI Mobile App Architecture

### MAUI Blazor Hybrid

The mobile app uses **.NET MAUI Blazor Hybrid**, which allows:
- Reuse of Blazor components from the web app
- Native platform capabilities (notifications, biometrics, GPS)
- Offline-first architecture with local SQLite database
- Background sync when connectivity restored

### Project Structure

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add Blazor Hybrid
        builder.Services.AddMauiBlazorWebView();
        
        #if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        #endif
        
        // Register services
        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddSingleton<ILocalDatabase, LocalDatabase>();
        builder.Services.AddTransient<ICharacterService, CharacterService>();
        builder.Services.AddTransient<ISessionService, SessionService>();
        
        // Add authentication
        builder.Services.AddAuthenticationStateDeserialization();
        
        return builder.Build();
    }
}
```

### MVVM ViewModels

```csharp
// ViewModels/CharacterListViewModel.cs
public partial class CharacterListViewModel : ObservableObject
{
    private readonly ICharacterService _characterService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private ObservableCollection<Character> characters = [];
    
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private string? errorMessage;
    
    public CharacterListViewModel(
        ICharacterService characterService,
        INavigationService navigationService)
    {
        _characterService = characterService;
        _navigationService = navigationService;
    }
    
    [RelayCommand]
    private async Task LoadCharactersAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        
        try
        {
            var result = await _characterService.GetCharactersAsync();
            Characters = new ObservableCollection<Character>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load characters: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadCharactersAsync();
        IsRefreshing = false;
    }
    
    [RelayCommand]
    private async Task SelectCharacterAsync(Character character)
    {
        await _navigationService.NavigateToAsync(
            "character-detail",
            new Dictionary<string, object> { ["Character"] = character });
    }
    
    [RelayCommand]
    private async Task CreateCharacterAsync()
    {
        await _navigationService.NavigateToAsync("character-create");
    }
}
```

### Platform-Specific Features

```csharp
// Services/PlatformService.cs
public class PlatformService : IPlatformService
{
    public async Task<bool> RequestNotificationPermissionAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }
    
    public async Task SendLocalNotificationAsync(string title, string message)
    {
        var notification = new NotificationRequest
        {
            Title = title,
            Description = message,
            NotificationId = Random.Shared.Next()
        };
        
        await LocalNotificationCenter.Current.Show(notification);
    }
    
    public async Task<bool> AuthenticateWithBiometricsAsync()
    {
        #if ANDROID || IOS
        var result = await BiometricAuthentication.AuthenticateAsync(
            new AuthenticationRequestConfiguration(
                "Authenticate",
                "Use biometrics to access your characters"));
        
        return result.Status == AuthenticationResultStatus.Succeeded;
        #else
        return true;
        #endif
    }
}
```

---

## SignalR Real-Time Communication

### Hub Implementation

```csharp
// Hubs/GameSessionHub.cs
public class GameSessionHub : Hub<IGameSessionClient>
{
    private readonly ISessionService _sessionService;
    private readonly IDiceRoller _diceRoller;
    private readonly ILlmDmService _dmService;
    private readonly ILogger<GameSessionHub> _logger;
    
    public GameSessionHub(
        ISessionService sessionService,
        IDiceRoller diceRoller,
        ILlmDmService dmService,
        ILogger<GameSessionHub> logger)
    {
        _sessionService = sessionService;
        _diceRoller = diceRoller;
        _dmService = dmService;
        _logger = logger;
    }
    
    public async Task JoinSession(int sessionId, int characterId)
    {
        _logger.LogInformation(
            "Player {ConnectionId} joining session {SessionId} with character {CharacterId}",
            Context.ConnectionId, sessionId, characterId);
        
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session is null)
            throw new HubException("Session not found");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        
        await Clients.Group($"session-{sessionId}")
            .PlayerJoined(Context.ConnectionId, characterId);
    }
    
    public async Task SendAction(int sessionId, PlayerAction action)
    {
        _logger.LogInformation(
            "Processing action in session {SessionId}: {Action}",
            sessionId, action.ActionText);
        
        // Get DM response
        var dmResponse = await _dmService.ProcessPlayerActionAsync(
            sessionId,
            action.ActionText);
        
        // Save to database
        await _sessionService.AddMessageAsync(sessionId, new Message
        {
            AuthorId = "DM",
            Role = MessageRole.DungeonMaster,
            Content = dmResponse.Content,
            Timestamp = DateTime.UtcNow
        });
        
        // Notify all clients
        await Clients.Group($"session-{sessionId}")
            .DungeonMasterResponse(dmResponse);
    }
    
    public async Task RollDice(int sessionId, string formula)
    {
        _logger.LogInformation(
            "Rolling dice in session {SessionId}: {Formula}",
            sessionId, formula);
        
        var result = _diceRoller.Roll(formula);
        
        // Save roll to database
        await _sessionService.LogDiceRollAsync(sessionId, new DiceRoll
        {
            SessionId = sessionId,
            RollerId = Context.ConnectionId,
            Formula = formula,
            IndividualRolls = result.IndividualRolls,
            Modifier = result.Modifier,
            Total = result.Total,
            Type = DiceRollType.Custom,
            Timestamp = DateTime.UtcNow
        });
        
        // Notify all clients
        await Clients.Group($"session-{sessionId}")
            .DiceRolled(Context.ConnectionId, result);
    }
    
    public async Task RequestInitiative(int sessionId)
    {
        var participants = await _sessionService.GetSessionParticipantsAsync(sessionId);
        var initiativeOrder = new List<InitiativeEntry>();
        
        foreach (var participant in participants)
        {
            var roll = _diceRoller.Roll("1d20");
            initiativeOrder.Add(new InitiativeEntry
            {
                CharacterId = participant.CharacterId,
                Roll = roll.Total
            });
        }
        
        var sorted = initiativeOrder.OrderByDescending(e => e.Roll).ToList();
        
        await Clients.Group($"session-{sessionId}")
            .InitiativeRolled(sorted);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            "Player {ConnectionId} disconnected",
            Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}

// Strongly-typed client interface
public interface IGameSessionClient
{
    Task PlayerJoined(string connectionId, int characterId);
    Task PlayerLeft(string connectionId);
    Task DungeonMasterResponse(DmResponse response);
    Task DiceRolled(string playerId, DiceRollResult result);
    Task InitiativeRolled(List<InitiativeEntry> order);
    Task TurnChanged(int currentCharacterId);
    Task SessionStateChanged(SessionState newState);
}
```

---

## API Endpoints (REST)

```csharp
// Controllers/CharactersController.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CharactersController : ControllerBase
{
    private readonly ICharacterService _characterService;
    
    public CharactersController(ICharacterService characterService)
    {
        _characterService = characterService;
    }
    
    /// <summary>
    /// Get all characters for the authenticated player
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CharacterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CharacterDto>>> GetCharacters()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var characters = await _characterService.GetCharactersByPlayerAsync(userId!);
        return Ok(characters);
    }
    
    /// <summary>
    /// Get a specific character by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> GetCharacter(int id)
    {
        var character = await _characterService.GetCharacterAsync(id);
        if (character is null)
            return NotFound();
        
        return Ok(character);
    }
    
    /// <summary>
    /// Create a new character
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CharacterDto>> CreateCharacter(
        [FromBody] CreateCharacterRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var character = await _characterService.CreateCharacterAsync(userId!, request);
        
        return CreatedAtAction(
            nameof(GetCharacter),
            new { id = character.Id },
            character);
    }
    
    /// <summary>
    /// Update character hit points
    /// </summary>
    [HttpPatch("{id}/hitpoints")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHitPoints(
        int id,
        [FromBody] UpdateHitPointsRequest request)
    {
        var success = await _characterService.UpdateHitPointsAsync(id, request.NewHitPoints);
        if (!success)
            return NotFound();
        
        return NoContent();
    }
    
    /// <summary>
    /// Level up a character
    /// </summary>
    [HttpPost("{id}/level-up")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> LevelUp(int id)
    {
        var character = await _characterService.LevelUpAsync(id);
        if (character is null)
            return NotFound();
        
        return Ok(character);
    }
}

// Controllers/SessionsController.cs
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    
    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }
    
    /// <summary>
    /// Create a new game session
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<SessionDto>> CreateSession(
        [FromBody] CreateSessionRequest request)
    {
        var session = await _sessionService.CreateSessionAsync(request);
        
        return CreatedAtAction(
            nameof(GetSession),
            new { id = session.Id },
            session);
    }
    
    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> GetSession(int id)
    {
        var session = await _sessionService.GetSessionAsync(id);
        if (session is null)
            return NotFound();
        
        return Ok(session);
    }
    
    /// <summary>
    /// Get session history (messages and rolls)
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(SessionHistoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SessionHistoryDto>> GetSessionHistory(
        int id,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var history = await _sessionService.GetSessionHistoryAsync(id, skip, take);
        return Ok(history);
    }
}
```

---

## Testing Strategy

### Unit Tests (xUnit)

```csharp
// Tests/DNDGame.UnitTests/Services/CharacterServiceTests.cs
public class CharacterServiceTests
{
    private readonly Mock<ICharacterRepository> _mockRepo;
    private readonly Mock<IDiceRoller> _mockDiceRoller;
    private readonly CharacterService _sut;
    
    public CharacterServiceTests()
    {
        _mockRepo = new Mock<ICharacterRepository>();
        _mockDiceRoller = new Mock<IDiceRoller>();
        _sut = new CharacterService(_mockRepo.Object, _mockDiceRoller.Object);
    }
    
    [Fact]
    public async Task CreateCharacter_WithValidData_ReturnsCharacter()
    {
        var request = new CreateCharacterRequest
        {
            Name = "Gandalf the Grey",
            Class = CharacterClass.Wizard,
            AbilityScores = new AbilityScores(10, 14, 12, 18, 16, 13)
        };
        
        _mockDiceRoller
            .Setup(d => d.Roll("1d6"))
            .Returns(new DiceRollResult("1d6", 4, [4], 0, DateTime.UtcNow));
        
        var result = await _sut.CreateCharacterAsync("user123", request);
        
        result.Should().NotBeNull();
        result.Name.Should().Be("Gandalf the Grey");
        result.Class.Should().Be(CharacterClass.Wizard);
        result.Level.Should().Be(1);
        result.HitPoints.Should().BeGreaterThan(0);
        
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Character>()), Times.Once);
    }
    
    [Theory]
    [InlineData(1, 2)]
    [InlineData(5, 3)]
    [InlineData(9, 4)]
    [InlineData(20, 6)]
    public void CalculateProficiencyBonus_ReturnsCorrectValue(int level, int expected)
    {
        var bonus = CharacterService.CalculateProficiencyBonus(level);
        bonus.Should().Be(expected);
    }
}
```

### Integration Tests

```csharp
// Tests/DNDGame.IntegrationTests/Api/CharactersControllerTests.cs
public class CharactersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public CharactersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetCharacters_ReturnsOkResult()
    {
        var response = await _client.GetAsync("/api/v1/characters");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var characters = await response.Content
            .ReadFromJsonAsync<IEnumerable<CharacterDto>>();
        
        characters.Should().NotBeNull();
    }
    
    [Fact]
    public async Task CreateCharacter_WithValidData_ReturnsCreated()
    {
        var request = new CreateCharacterRequest
        {
            Name = "Test Character",
            Class = CharacterClass.Fighter,
            AbilityScores = new AbilityScores(15, 14, 13, 12, 10, 8)
        };
        
        var response = await _client.PostAsJsonAsync("/api/v1/characters", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var character = await response.Content.ReadFromJsonAsync<CharacterDto>();
        character.Should().NotBeNull();
        character!.Name.Should().Be("Test Character");
    }
}
```

### Blazor Component Tests (bUnit)

```csharp
// Tests/DNDGame.BlazorTests/Components/DiceRollerTests.cs
public class DiceRollerTests : TestContext
{
    [Fact]
    public void DiceRoller_RendersCorrectly()
    {
        var cut = RenderComponent<DiceRoller>();
        
        cut.Find("input[type='text']").Should().NotBeNull();
        cut.Find("button").Should().NotBeNull();
    }
    
    [Fact]
    public void DiceRoller_RollButton_InvokesCallback()
    {
        DiceRollResult? capturedResult = null;
        var cut = RenderComponent<DiceRoller>(parameters => parameters
            .Add(p => p.OnRollDice, result => capturedResult = result));
        
        var input = cut.Find("input[type='text']");
        input.Change("2d20+5");
        
        var button = cut.Find("button");
        button.Click();
        
        capturedResult.Should().NotBeNull();
        capturedResult!.Formula.Should().Be("2d20+5");
    }
}
```

---

## Deployment Architecture

### Development Environment
- **API**: Local Kestrel server (HTTPS on port 7001)
- **Database**: SQLite for rapid development
- **LLM**: OpenAI API with development key

### Production Environment (Docker-based)
- **API**: Multi-stage Docker image with ASP.NET Core runtime
- **Database**: PostgreSQL or SQL Server in Docker container
- **Cache**: Redis Docker container for session state and pub/sub
- **Storage**: Local volume mounts or S3-compatible object storage
- **Reverse Proxy**: Nginx or Traefik for SSL termination and load balancing
- **Monitoring**: Prometheus + Grafana stack in containers
- **Container Orchestration**: Docker Compose for single-host or Kubernetes for multi-host

### CI/CD Pipeline (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
name: Build and Deploy DNDGame

on:
  push:
    branches: [ main, develop ]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --configuration Release
    
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha
    
    - name: Build and push API Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./src/DNDGame.API/Dockerfile
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
    
    - name: Build and push Blazor Server Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./src/DNDGame.Web.Server/Dockerfile
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}-blazor:${{ github.sha }}
    
    - name: Deploy to Production
      if: github.ref == 'refs/heads/main'
      run: |
        # SSH into production server and update containers
        echo "${{ secrets.DEPLOY_SSH_KEY }}" > deploy_key
        chmod 600 deploy_key
        ssh -i deploy_key -o StrictHostKeyChecking=no ${{ secrets.DEPLOY_USER }}@${{ secrets.DEPLOY_HOST }} '
          cd /opt/dndgame &&
          docker-compose pull &&
          docker-compose up -d --remove-orphans
        '
```

### Docker Configuration

#### API Dockerfile
```dockerfile
# src/DNDGame.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DNDGame.API/DNDGame.API.csproj", "src/DNDGame.API/"]
COPY ["src/DNDGame.Application/DNDGame.Application.csproj", "src/DNDGame.Application/"]
COPY ["src/DNDGame.Core/DNDGame.Core.csproj", "src/DNDGame.Core/"]
COPY ["src/DNDGame.Infrastructure/DNDGame.Infrastructure.csproj", "src/DNDGame.Infrastructure/"]
COPY ["src/DNDGame.Shared/DNDGame.Shared.csproj", "src/DNDGame.Shared/"]
RUN dotnet restore "src/DNDGame.API/DNDGame.API.csproj"
COPY . .
WORKDIR "/src/src/DNDGame.API"
RUN dotnet build "DNDGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DNDGame.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DNDGame.API.dll"]
```

#### Docker Compose Configuration
```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    image: ghcr.io/sdchesney/dndgame-dotnet:latest
    container_name: dndgame-api
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080;https://+:8081
      - ConnectionStrings__DefaultConnection=Host=database;Port=5432;Database=dndgame;Username=dnduser;Password=${DB_PASSWORD}
      - Redis__ConnectionString=redis:6379
      - Jwt__Key=${JWT_SECRET_KEY}
      - OpenAI__ApiKey=${OPENAI_API_KEY}
    depends_on:
      - database
      - redis
    networks:
      - dndgame-network
    volumes:
      - ./logs:/app/logs
      - ./uploads:/app/uploads

  blazor-server:
    image: ghcr.io/sdchesney/dndgame-dotnet-blazor:latest
    container_name: dndgame-blazor
    ports:
      - "8082:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ApiSettings__BaseUrl=http://api:8080
    depends_on:
      - api
    networks:
      - dndgame-network

  database:
    image: postgres:15-alpine
    container_name: dndgame-db
    environment:
      - POSTGRES_DB=dndgame
      - POSTGRES_USER=dnduser
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init.sql:/docker-entrypoint-initdb.d/init.sql
    ports:
      - "5432:5432"
    networks:
      - dndgame-network

  redis:
    image: redis:7-alpine
    container_name: dndgame-redis
    command: redis-server --appendonly yes
    volumes:
      - redis_data:/data
    ports:
      - "6379:6379"
    networks:
      - dndgame-network

  nginx:
    image: nginx:alpine
    container_name: dndgame-nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/ssl:/etc/nginx/ssl
      - ./nginx/logs:/var/log/nginx
    depends_on:
      - blazor-server
      - api
    networks:
      - dndgame-network

networks:
  dndgame-network:
    driver: bridge

volumes:
  postgres_data:
  redis_data:
```

#### Production Environment Configuration
```bash
# .env.production
DB_PASSWORD=your_secure_database_password
JWT_SECRET_KEY=your_jwt_secret_key_32_characters_min
OPENAI_API_KEY=sk-your_openai_api_key
```

---

## Next Steps (Implementation Roadmap)

### Phase 1: Foundation (Weeks 1-2)
- [ ] Create .NET solution structure with all projects
- [ ] Set up Entity Framework Core with initial migrations
- [ ] Implement core domain models (Character, Session, Player)
- [ ] Create basic API endpoints for characters
- [ ] Set up authentication with JWT

### Phase 2: Game Logic (Weeks 3-4)
- [ ] Implement dice rolling system with validation
- [ ] Build rules engine for ability checks and saving throws
- [ ] Create character service with level-up logic
- [ ] Implement combat initiative system
- [ ] Add hit point and armor class calculations

### Phase 3: LLM Integration (Weeks 5-6)
- [ ] Set up Semantic Kernel or direct LLM client
- [ ] Design DM prompt templates
- [ ] Implement conversation context management
- [ ] Add content moderation layer
- [ ] Create retry and rate limiting policies

### Phase 4: Real-Time Features (Weeks 7-8)
- [ ] Implement SignalR hubs for game sessions
- [ ] Build WebSocket client in Blazor
- [ ] Add multiplayer session management
- [ ] Implement turn-based combat synchronization
- [ ] Create real-time dice rolling

### Phase 5: Blazor Web UI (Weeks 9-10)
- [ ] Design responsive layout with CSS/Tailwind
- [ ] Build character sheet component
- [ ] Create chat panel with DM/player differentiation
- [ ] Implement dice roller UI
- [ ] Add initiative tracker component
- [ ] Build session lobby and management

### Phase 6: MAUI Mobile App (Weeks 11-12)
- [ ] Set up MAUI Blazor Hybrid project
- [ ] Implement ViewModels with MVVM Toolkit
- [ ] Create platform-specific services
- [ ] Add offline data sync
- [ ] Implement push notifications
- [ ] Build character management screens

### Phase 7: Testing & Polish (Weeks 13-14)
- [ ] Write comprehensive unit tests
- [ ] Add integration tests for API
- [ ] Create Blazor component tests with bUnit
- [ ] Load test SignalR hubs
- [ ] Performance optimization
- [ ] Security audit

### Phase 8: Deployment (Weeks 15-16)
- [ ] Create Docker images for API and Blazor Server
- [ ] Set up production server with Docker and Docker Compose
- [ ] Configure reverse proxy (Nginx) with SSL certificates
- [ ] Set up PostgreSQL and Redis containers
- [ ] Configure CI/CD pipelines with GitHub Actions
- [ ] Deploy containerized applications
- [ ] Submit MAUI apps to app stores
- [ ] Set up monitoring with Prometheus/Grafana stack

---

## Licensing & IP

- Avoid distributing non‑SRD or proprietary D&D content.
- Provide original text, SRD‑compatible mechanics, or user‑supplied homebrew.
- Include a clear disclaimer in README and the app UI.
- This project is for educational and non-commercial purposes.

---

## Additional Resources

- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [SignalR Documentation](https://learn.microsoft.com/aspnet/core/signalr/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Semantic Kernel](https://learn.microsoft.com/semantic-kernel/)
- [D&D 5e SRD](https://dnd.wizards.com/resources/systems-reference-document)

---

**Ready to start building!** This architecture provides a solid foundation for a scalable, cross-platform D&D-style RPG powered by LLMs, using modern .NET technologies. 🎲🐉
