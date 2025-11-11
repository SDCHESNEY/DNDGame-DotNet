# DNDGame .NET Development Roadmap

## Overview

This roadmap outlines the 16-week implementation plan for the LLM Dungeon Master RPG using .NET Core, Blazor, and .NET MAUI. Each phase includes acceptance criteria, test coverage goals, and expected deliverables.

---

## Phase 1: Foundation (Weeks 1-2) ✅ COMPLETED

### Goals
Establish the core .NET solution structure with domain models, database infrastructure, and basic API endpoints.

### Tasks
- [x] Create .NET solution structure with all projects (Core, Infrastructure, Application, API, Shared, Tests)
- [x] Set up Entity Framework Core with initial migrations
- [x] Implement core domain models (Character, Session, Player, Message, DiceRoll)
- [x] Create basic API endpoints for characters and sessions
- [x] Configure dependency injection in Program.cs
- [x] Add FluentValidation for request validation
- [x] Set up xUnit test projects with Moq and FluentAssertions
- [x] Implement comprehensive unit tests (48 tests passing)

### Files to Create
```
DNDGame.sln
src/
├── DNDGame.Core/
│   ├── Entities/
│   │   ├── Character.cs
│   │   ├── Session.cs
│   │   ├── Player.cs
│   │   ├── Message.cs
│   │   └── DiceRoll.cs
│   ├── Interfaces/
│   │   ├── ICharacterService.cs
│   │   ├── ISessionService.cs
│   │   └── IDiceRoller.cs
│   └── ValueObjects/
│       ├── AbilityScores.cs
│       └── CharacterClass.cs
├── DNDGame.Infrastructure/
│   ├── Data/
│   │   ├── DndGameContext.cs
│   │   └── Configurations/
│   │       ├── CharacterConfiguration.cs
│   │       └── SessionConfiguration.cs
│   └── Repositories/
│       ├── CharacterRepository.cs
│       └── SessionRepository.cs
├── DNDGame.Application/
│   ├── Services/
│   │   └── CharacterService.cs
│   ├── DTOs/
│   │   ├── CreateCharacterRequest.cs
│   │   └── CharacterDto.cs
│   └── Validators/
│       └── CreateCharacterValidator.cs
└── DNDGame.API/
    ├── Controllers/
    │   └── CharactersController.cs
    ├── Program.cs
    └── appsettings.json

tests/
└── DNDGame.UnitTests/
    ├── Services/
    │   └── CharacterServiceTests.cs
    └── Validators/
        └── CreateCharacterValidatorTests.cs
```

### Acceptance Criteria
- [x] Solution builds without errors
- [x] Database can be created and migrated with `dotnet ef database update`
- [x] Character CRUD operations work via API endpoints
- [x] Session CRUD operations work via API endpoints
- [x] All entities have proper EF Core configurations
- [x] Dependency injection container is properly configured
- [x] API returns proper status codes (200, 201, 400, 404, 500)
- [x] Request validation works with FluentValidation
- [x] Problem Details (RFC 7807) implemented for error responses
- [x] XML documentation on all controller methods
- [x] Structured logging with ILogger

### Test Results Summary
**Target**: 30+ unit tests  
**Achieved**: 48 tests passing ✅

Actual coverage:
- **Domain Models**: 9 tests
  - AbilityScores modifier calculations (7 tests)
  - Character proficiency bonus by level (6 tests)
  - Character entity initialization (2 tests)
  - Session entity initialization (2 tests)
- **Services**: 14 tests
  - CharacterService CRUD operations (7 tests)
  - SessionService CRUD operations (7 tests)
  - Proper DTO mapping
  - Error handling for not found cases
- **Validators**: 11 tests
  - CreateCharacterRequestValidator (8 tests)
  - CreateSessionRequestValidator (3 tests)
  - Valid and invalid input scenarios

### Implementation Details
**Technologies Used:**
- .NET 9.0
- Entity Framework Core 9.0.0 with SQL Server
- FluentValidation 12.1.0 with DependencyInjectionExtensions
- xUnit, Moq 4.20.72, FluentAssertions 8.8.0

**Architecture:**
- Clean Architecture with Domain, Infrastructure, Application, and API layers
- Repository pattern with async/await
- CQRS-style separation with DTOs
- Value Objects (AbilityScores)
- Rich domain models with calculated properties

**API Endpoints Implemented:**
- `GET /api/v1/characters/{id}` - Get character by ID
- `GET /api/v1/characters/player/{playerId}` - Get all characters for a player
- `POST /api/v1/characters/player/{playerId}` - Create new character
- `PUT /api/v1/characters/{id}` - Update character
- `DELETE /api/v1/characters/{id}` - Delete character
- `GET /api/v1/sessions/{id}` - Get session by ID
- `GET /api/v1/sessions` - Get all sessions
- `POST /api/v1/sessions` - Create new session
- `PATCH /api/v1/sessions/{id}/state` - Update session state
- `DELETE /api/v1/sessions/{id}` - Delete session

**Notes:**
- JWT authentication deferred to Phase 2 (will integrate with LLM chat features)
- Web (Blazor) and MAUI projects created but empty (Phase 5-6)
- Initial migration created and ready for database deployment

**Success Criteria**:
- ✅ 100% test pass rate
- ✅ 80%+ code coverage on services
- ✅ All validators have test cases for each rule

---

## Phase 2: Game Logic (Weeks 3-4)

### Goals
Implement the D&D 5e rules engine including dice rolling, ability checks, combat mechanics, and character progression.

### Tasks
- [ ] Implement cryptographically secure dice rolling system
- [ ] Build rules engine for ability checks and saving throws
- [ ] Create character service with level-up logic
- [ ] Implement combat initiative system
- [ ] Add hit point and armor class calculations
- [ ] Create proficiency bonus calculations
- [ ] Implement advantage/disadvantage mechanics
- [ ] Add attack roll and damage resolution
- [ ] Create conditions system (stunned, prone, etc.)

### Files to Create
```csharp
// DNDGame.Application/Services/DiceRollerService.cs
public class DiceRollerService : IDiceRoller
{
    DiceRollResult Roll(string formula);
    DiceRollResult RollWithAdvantage(string formula);
    DiceRollResult RollWithDisadvantage(string formula);
}

// DNDGame.Application/Services/RulesEngineService.cs
public class RulesEngineService : IRulesEngine
{
    CheckResult ResolveAbilityCheck(int abilityScore, int dc, bool proficient);
    CheckResult ResolveSavingThrow(Character character, AbilityType ability, int dc);
    AttackResult ResolveAttack(int attackBonus, int targetAC, AdvantageType advantage);
    int CalculateDamage(string damageFormula, bool critical);
}

// DNDGame.Application/Services/CombatService.cs
public class CombatService : ICombatService
{
    Task<InitiativeOrder> RollInitiative(int sessionId);
    Task<AttackResult> ResolveAttack(int attackerId, int defenderId);
    Task<bool> ApplyDamage(int characterId, int damage);
    Task<bool> ApplyHealing(int characterId, int healing);
}

// DNDGame.Core/ValueObjects/DiceFormula.cs
public record DiceFormula(int Count, int Sides, int Modifier)
{
    public static DiceFormula Parse(string formula);
}

// DNDGame.Core/Entities/Condition.cs
public class Condition
{
    public ConditionType Type { get; set; }
    public int Duration { get; set; }
    public int SaveDC { get; set; }
}

// DNDGame.Core/Enums/ConditionType.cs
public enum ConditionType
{
    Blinded, Charmed, Deafened, Frightened, Grappled,
    Incapacitated, Invisible, Paralyzed, Petrified, Poisoned,
    Prone, Restrained, Stunned, Unconscious, Exhausted
}
```

### API Endpoints
- `POST /api/v1/dice/roll` - Roll dice with formula
- `POST /api/v1/combat/initiative` - Roll initiative for session
- `POST /api/v1/combat/attack` - Resolve attack between characters
- `POST /api/v1/combat/damage` - Apply damage to character
- `POST /api/v1/combat/healing` - Apply healing to character
- `GET /api/v1/characters/{id}/combat-stats` - Get combat-relevant stats
- `POST /api/v1/characters/{id}/level-up` - Level up a character

### Acceptance Criteria
- [ ] Dice rolls use `RandomNumberGenerator` for cryptographic security
- [ ] All dice formulas parse correctly (1d20, 2d6+3, 3d8-1)
- [ ] Advantage/disadvantage rolls twice and takes correct value
- [ ] Ability checks compare correctly against DC
- [ ] Attack rolls account for proficiency bonus
- [ ] Critical hits double damage dice (not modifiers)
- [ ] Hit points cannot go below 0 or above maximum
- [ ] Initiative order sorts by initiative value
- [ ] All 15 D&D 5e conditions are implemented
- [ ] Proficiency bonus scales correctly by level (2-6)

### Test Results Summary
**Target**: 60+ unit tests

Expected coverage:
- **DiceRollerService**: 15 tests
  - Basic dice rolls (d4, d6, d8, d10, d12, d20, d100)
  - Complex formulas (2d6+3, 4d8-2)
  - Advantage/disadvantage mechanics
  - Edge cases (0 dice, invalid formulas)
- **RulesEngineService**: 20 tests
  - Ability checks (all 6 abilities)
  - Saving throws with proficiency
  - Attack roll resolution
  - Critical hits and fumbles
  - Damage calculation
  - Advantage/disadvantage application
- **CombatService**: 15 tests
  - Initiative rolling
  - Attack resolution
  - Damage application
  - Healing application
  - Death mechanics (0 HP)
- **ConditionService**: 10 tests
  - Condition application
  - Duration tracking
  - Condition effects on rolls
  - Multiple conditions interaction

**Success Criteria**:
- ✅ 100% test pass rate
- ✅ 90%+ code coverage on game logic
- ✅ All dice rolls are verifiably random
- ✅ Rules match D&D 5e SRD accurately

---

## Phase 3: LLM Integration (Weeks 5-6)

### Goals
Integrate LLM providers (OpenAI, Azure OpenAI, Anthropic) for the AI Dungeon Master with streaming responses, context management, and content moderation.

### Tasks
- [ ] Set up Semantic Kernel or direct HTTP clients
- [ ] Design system prompt templates for DM
- [ ] Implement conversation context management
- [ ] Add streaming response handling
- [ ] Create content moderation layer
- [ ] Implement retry policies with Polly
- [ ] Add rate limiting with AspNetCoreRateLimit
- [ ] Create token usage tracking
- [ ] Build prompt template system
- [ ] Add LLM response caching

### Files to Create
```csharp
// DNDGame.Infrastructure/Services/LlmDmService.cs
public class LlmDmService : ILlmDmService
{
    Task<DmResponse> GenerateResponseAsync(SessionContext context, string playerAction);
    IAsyncEnumerable<string> StreamResponseAsync(SessionContext context, string playerAction);
    Task<string> GenerateNpcDialogueAsync(NpcContext npc, string playerMessage);
    Task<string> DescribeSceneAsync(LocationContext location);
}

// DNDGame.Infrastructure/Services/OpenAiProvider.cs
public class OpenAiProvider : ILlmProvider
{
    Task<string> CompleteAsync(string systemPrompt, string userMessage);
    IAsyncEnumerable<string> StreamCompleteAsync(string systemPrompt, string userMessage);
}

// DNDGame.Application/Services/PromptTemplateService.cs
public class PromptTemplateService : IPromptTemplateService
{
    string GetSystemPrompt(CampaignSettings settings);
    string GetCombatPrompt(CombatState state);
    string GetExplorationPrompt(LocationContext location);
    string GetNpcPrompt(NpcContext npc);
}

// DNDGame.Application/Services/ContentModerationService.cs
public class ContentModerationService : IContentModerationService
{
    Task<ModerationResult> ModerateInputAsync(string content);
    Task<ModerationResult> ModerateOutputAsync(string content);
    Task<string> SanitizeContentAsync(string content);
}

// DNDGame.Core/Models/SessionContext.cs
public record SessionContext(
    int SessionId,
    List<Message> RecentMessages,
    List<Character> ActiveCharacters,
    string CurrentScene,
    Dictionary<string, object> WorldFlags
);

// DNDGame.Core/Models/DmResponse.cs
public record DmResponse(
    string Content,
    MessageRole Role,
    int TokensUsed,
    TimeSpan ResponseTime,
    List<string> SuggestedActions
);
```

### Configuration
```json
// appsettings.json
{
  "LLM": {
    "Provider": "OpenAI",
    "ApiKey": "sk-...",
    "Model": "gpt-4-turbo-preview",
    "MaxTokens": 500,
    "Temperature": 0.7,
    "StreamResponses": true
  },
  "RateLimit": {
    "RequestsPerMinute": 20,
    "TokensPerMinute": 100000
  },
  "ContentModeration": {
    "Enabled": true,
    "BlockNSFW": true,
    "BlockHarassment": true
  }
}
```

### Acceptance Criteria
- [ ] LLM responds within 5 seconds for exploration
- [ ] Streaming responses display incrementally
- [ ] Context includes last 10 messages minimum
- [ ] Content moderation blocks inappropriate content
- [ ] Retry policy handles transient failures (3 retries with exponential backoff)
- [ ] Rate limiting prevents API abuse
- [ ] Token usage is tracked per session
- [ ] Multiple LLM providers are supported (OpenAI, Azure, Anthropic)
- [ ] Responses maintain character consistency
- [ ] DM never contradicts established facts

### Test Results Summary
**Target**: 40+ unit tests

Expected coverage:
- **LlmDmService**: 15 tests
  - Response generation
  - Streaming responses
  - Context management
  - Error handling
  - Token tracking
- **PromptTemplateService**: 10 tests
  - System prompt generation
  - Combat prompts
  - Exploration prompts
  - NPC dialogue prompts
  - Template variable substitution
- **ContentModerationService**: 10 tests
  - NSFW detection
  - Harassment detection
  - Content sanitization
  - False positive handling
- **OpenAiProvider**: 5 tests
  - API integration
  - Streaming
  - Error handling
  - Rate limiting

**Integration Tests**: 10 tests
- End-to-end LLM conversations
- Multi-turn context handling
- Streaming to SignalR clients
- Failure recovery scenarios

**Success Criteria**:
- ✅ 95%+ test pass rate
- ✅ Average response time <5s
- ✅ Content moderation accuracy >90%
- ✅ Zero API key leaks in logs

---

## Phase 4: Real-Time Features (Weeks 7-8)

### Goals
Implement SignalR hubs for real-time multiplayer gameplay with WebSocket communication, turn-based combat, and synchronized game state.

### Tasks
- [ ] Implement SignalR hubs for game sessions
- [ ] Build WebSocket client in Blazor
- [ ] Add multiplayer session management
- [ ] Implement turn-based combat synchronization
- [ ] Create real-time dice rolling
- [ ] Add presence tracking (online/offline)
- [ ] Implement reconnection logic
- [ ] Create group messaging for sessions
- [ ] Add combat state broadcasting
- [ ] Build initiative order updates

### Files to Create
```csharp
// DNDGame.API/Hubs/GameSessionHub.cs
public class GameSessionHub : Hub<IGameSessionClient>
{
    Task JoinSession(int sessionId, int characterId);
    Task LeaveSession(int sessionId);
    Task SendMessage(int sessionId, string message);
    Task SendAction(int sessionId, PlayerAction action);
    Task RollDice(int sessionId, string formula);
    Task RequestInitiative(int sessionId);
    Task EndTurn(int sessionId, int characterId);
}

// DNDGame.API/Hubs/IGameSessionClient.cs
public interface IGameSessionClient
{
    Task PlayerJoined(string connectionId, int characterId);
    Task PlayerLeft(string connectionId);
    Task ReceiveMessage(MessageDto message);
    Task DungeonMasterResponse(DmResponse response);
    Task DiceRolled(string playerId, DiceRollResult result);
    Task InitiativeRolled(List<InitiativeEntry> order);
    Task TurnChanged(int currentCharacterId);
    Task SessionStateChanged(SessionState newState);
    Task CombatStarted(CombatEncounter encounter);
    Task CombatEnded(CombatResult result);
}

// DNDGame.Application/Services/SessionService.cs
public class SessionService : ISessionService
{
    Task<Session> CreateSessionAsync(CreateSessionRequest request);
    Task<Session> GetSessionAsync(int id);
    Task<bool> JoinSessionAsync(int sessionId, int characterId);
    Task<bool> LeaveSessionAsync(int sessionId, int characterId);
    Task<Message> SaveMessageAsync(int sessionId, string content, MessageRole role);
    Task<DiceRollResult> RollDiceAsync(int sessionId, string formula);
}

// DNDGame.Application/Services/PresenceService.cs
public class PresenceService : IPresenceService
{
    Task TrackConnectionAsync(int sessionId, int playerId, string connectionId);
    Task RemoveConnectionAsync(string connectionId);
    Task<List<PlayerPresence>> GetActivePlayersAsync(int sessionId);
    Task<bool> IsPlayerOnlineAsync(int playerId);
}
```

### SignalR Configuration
```csharp
// Program.cs
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
    options.StreamBufferCapacity = 10;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

app.MapHub<GameSessionHub>("/hubs/game-session");
```

### Blazor Client Setup
```csharp
// Blazor Components/Pages/Session.razor.cs
private HubConnection? hubConnection;

protected override async Task OnInitializedAsync()
{
    hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri("/hubs/game-session"))
        .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) })
        .Build();

    hubConnection.On<MessageDto>("ReceiveMessage", HandleMessage);
    hubConnection.On<DiceRollResult>("DiceRolled", HandleDiceRoll);
    hubConnection.On<List<InitiativeEntry>>("InitiativeRolled", HandleInitiative);
    hubConnection.Reconnecting += OnReconnecting;
    hubConnection.Reconnected += OnReconnected;
    hubConnection.Closed += OnClosed;

    await hubConnection.StartAsync();
    await hubConnection.InvokeAsync("JoinSession", SessionId, CharacterId);
}
```

### Acceptance Criteria
- [ ] Players can join/leave sessions via SignalR
- [ ] Messages broadcast to all session participants
- [ ] Dice rolls are synchronized across clients
- [ ] Initiative order updates in real-time
- [ ] Turn changes notify all players
- [ ] Presence tracking shows online/offline status
- [ ] Automatic reconnection works within 30 seconds
- [ ] Combat state synchronizes correctly
- [ ] DM responses stream to all players
- [ ] No message loss during reconnection

### Test Results Summary
**Target**: 50+ integration tests

Expected coverage:
- **GameSessionHub**: 20 tests
  - Join/leave session
  - Message broadcasting
  - Dice roll synchronization
  - Initiative updates
  - Turn management
  - Error handling
- **SessionService**: 15 tests
  - Session CRUD
  - Player management
  - Message persistence
  - State synchronization
- **PresenceService**: 10 tests
  - Connection tracking
  - Online/offline status
  - Timeout handling
- **SignalR Integration**: 5 tests
  - Multiple concurrent connections
  - Group messaging
  - Reconnection scenarios
  - Message ordering

**Performance Tests**: 5 tests
- 10 concurrent sessions
- 50 messages per second
- Latency <100ms
- Memory usage under load

**Success Criteria**:
- ✅ 100% test pass rate
- ✅ <100ms message latency
- ✅ Supports 50+ concurrent sessions
- ✅ Reconnection success rate >95%

---

## Phase 5: Blazor Web UI (Weeks 9-10)

### Goals
Build the Blazor web application with responsive components, real-time updates, and beautiful game interfaces.

### Tasks
- [ ] Design responsive layout with CSS/Tailwind
- [ ] Build character sheet component
- [ ] Create chat panel with DM/player differentiation
- [ ] Implement dice roller UI
- [ ] Add initiative tracker component
- [ ] Build session lobby and management
- [ ] Create inventory management component
- [ ] Add spell slots tracker
- [ ] Implement conditions display
- [ ] Build character creation wizard

### Files to Create
```razor
<!-- DNDGame.Shared/Components/CharacterSheet.razor -->
@inherits ComponentBase

<div class="character-sheet">
    <CharacterHeader Character="@Character" />
    <AbilityScores Abilities="@Character.AbilityScores" />
    <CombatStats HP="@Character.HitPoints" MaxHP="@Character.MaxHitPoints" AC="@Character.ArmorClass" />
    <Skills CharacterSkills="@Character.Skills" ProficiencyBonus="@Character.ProficiencyBonus" />
    <InventoryPanel Items="@Character.Inventory" OnItemUse="HandleItemUse" />
</div>

@code {
    [Parameter]
    public Character Character { get; set; } = null!;
    
    [Parameter]
    public EventCallback<InventoryItem> OnItemUse { get; set; }
}

<!-- DNDGame.Shared/Components/DiceRoller.razor -->
<div class="dice-roller">
    <input type="text" @bind="diceFormula" placeholder="1d20+5" />
    <button @onclick="RollDiceAsync">Roll</button>
    
    @if (lastRoll is not null)
    {
        <div class="roll-result @GetRollClass()">
            <span class="formula">@lastRoll.Formula</span>
            <span class="total">@lastRoll.Total</span>
            <span class="breakdown">[@string.Join(", ", lastRoll.IndividualRolls)] + @lastRoll.Modifier</span>
        </div>
    }
</div>

<!-- DNDGame.Shared/Components/ChatPanel.razor -->
<div class="chat-panel">
    <div class="messages" @ref="messagesContainer">
        @foreach (var message in Messages)
        {
            <div class="message @GetMessageClass(message)">
                <span class="author">@GetAuthorName(message)</span>
                <span class="timestamp">@message.Timestamp.ToString("HH:mm")</span>
                <div class="content">@message.Content</div>
            </div>
        }
    </div>
    
    <div class="input-area">
        <textarea @bind="currentMessage" @onkeydown="HandleKeyDown" placeholder="Describe your action..."></textarea>
        <button @onclick="SendMessageAsync">Send</button>
    </div>
</div>

<!-- DNDGame.Shared/Components/InitiativeTracker.razor -->
<div class="initiative-tracker">
    <h3>Initiative Order</h3>
    @foreach (var entry in InitiativeOrder)
    {
        <div class="initiative-entry @(entry.CharacterId == CurrentTurn ? "current-turn" : "")">
            <span class="initiative-value">@entry.InitiativeRoll</span>
            <span class="character-name">@entry.CharacterName</span>
            <span class="hp-indicator">HP: @entry.CurrentHP / @entry.MaxHP</span>
            @if (entry.Conditions.Any())
            {
                <div class="conditions">
                    @foreach (var condition in entry.Conditions)
                    {
                        <span class="condition-badge">@condition</span>
                    }
                </div>
            }
        </div>
    }
</div>
```

### Component Hierarchy
```
App.razor
├── Router
├── MainLayout.razor
│   ├── NavMenu.razor
│   └── RouteView
│       ├── Home.razor
│       ├── Characters.razor
│       │   ├── CharacterList.razor
│       │   └── CharacterCreate.razor
│       └── Session.razor
│           ├── ChatPanel.razor
│           ├── DiceRoller.razor
│           ├── CharacterSheet.razor
│           ├── InitiativeTracker.razor
│           ├── InventoryPanel.razor
│           └── ConditionsPanel.razor
```

### Styling
- Use **Tailwind CSS** or **MudBlazor** for component library
- Responsive design (mobile-first)
- Dark mode support
- Accessibility (ARIA labels, keyboard navigation)

### Acceptance Criteria
- [ ] Character sheet displays all stats correctly
- [ ] Chat updates in real-time via SignalR
- [ ] Dice roller works with validation
- [ ] Initiative tracker sorts by initiative value
- [ ] UI is responsive on mobile (320px+)
- [ ] Dark mode toggle works
- [ ] Components are keyboard accessible
- [ ] Loading states display correctly
- [ ] Error messages are user-friendly
- [ ] UI updates within 100ms of SignalR event

### Test Results Summary
**Target**: 40+ component tests (bUnit)

Expected coverage:
- **CharacterSheet**: 8 tests
  - Render with valid character
  - Display all ability scores
  - Update HP display
  - Inventory interaction
- **ChatPanel**: 10 tests
  - Render messages
  - Send message
  - Scroll to bottom on new message
  - Differentiate DM vs player messages
  - Handle long messages
- **DiceRoller**: 8 tests
  - Parse dice formula
  - Roll dice callback
  - Display result
  - Handle invalid formulas
  - Critical/fumble styling
- **InitiativeTracker**: 6 tests
  - Render initiative order
  - Highlight current turn
  - Display conditions
  - Update on turn change
- **Integration**: 8 tests
  - Full session page render
  - SignalR connection
  - Real-time updates
  - Navigation flows

**Success Criteria**:
- ✅ 100% component render tests pass
- ✅ All interactive elements have tests
- ✅ Accessibility score >90 (Lighthouse)
- ✅ Mobile rendering verified

---

## Phase 6: MAUI Mobile App (Weeks 11-12)

### Goals
Build the .NET MAUI Blazor Hybrid mobile application with native capabilities, offline support, and component reuse.

### Tasks
- [ ] Set up MAUI Blazor Hybrid project
- [ ] Implement ViewModels with MVVM Toolkit
- [ ] Create platform-specific services
- [ ] Add offline data sync with SQLite
- [ ] Implement push notifications
- [ ] Build character management screens
- [ ] Add biometric authentication
- [ ] Create native navigation
- [ ] Implement file picker for character import
- [ ] Add camera integration for character portraits

### Files to Create
```csharp
// DNDGame.MauiApp/MauiProgram.cs
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

        builder.Services.AddMauiBlazorWebView();
        
        // Platform services
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IBiometricService, BiometricService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        
        // ViewModels
        builder.Services.AddTransient<CharacterListViewModel>();
        builder.Services.AddTransient<CharacterDetailViewModel>();
        builder.Services.AddTransient<SessionViewModel>();
        
        // Offline sync
        builder.Services.AddSingleton<IOfflineSyncService, OfflineSyncService>();
        builder.Services.AddDbContext<LocalDatabaseContext>(options =>
            options.UseSqlite($"Filename={FileSystem.AppDataDirectory}/dndgame.db"));
        
        return builder.Build();
    }
}

// DNDGame.MauiApp/ViewModels/CharacterListViewModel.cs
public partial class CharacterListViewModel : ObservableObject
{
    private readonly ICharacterService _characterService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private ObservableCollection<Character> characters = [];
    
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string? errorMessage;
    
    [RelayCommand]
    private async Task LoadCharactersAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _characterService.GetAllCharactersAsync();
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
    private async Task SelectCharacterAsync(Character character)
    {
        await _navigationService.NavigateToAsync($"character-detail?id={character.Id}");
    }
}

// DNDGame.MauiApp/Services/NotificationService.cs
public class NotificationService : INotificationService
{
    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }
    
    public async Task ShowNotificationAsync(string title, string message)
    {
        var notification = new NotificationRequest
        {
            NotificationId = Random.Shared.Next(),
            Title = title,
            Description = message,
            BadgeNumber = 1
        };
        
        await LocalNotificationCenter.Current.Show(notification);
    }
    
    public async Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime)
    {
        var notification = new NotificationRequest
        {
            NotificationId = Random.Shared.Next(),
            Title = title,
            Description = message,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = scheduledTime
            }
        };
        
        await LocalNotificationCenter.Current.Show(notification);
    }
}

// DNDGame.MauiApp/Services/OfflineSyncService.cs
public class OfflineSyncService : IOfflineSyncService
{
    private readonly LocalDatabaseContext _localDb;
    private readonly ICharacterService _characterService;
    
    public async Task<bool> SyncCharactersAsync()
    {
        if (!Connectivity.NetworkAccess == NetworkAccess.Internet)
            return false;
        
        var localCharacters = await _localDb.Characters.ToListAsync();
        var remoteCharacters = await _characterService.GetAllCharactersAsync();
        
        // Merge logic here
        
        await _localDb.SaveChangesAsync();
        return true;
    }
    
    public async Task<Character?> GetCharacterOfflineAsync(int id)
    {
        return await _localDb.Characters.FindAsync(id);
    }
}
```

### XAML Layouts
```xml
<!-- DNDGame.MauiApp/Pages/CharacterListPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:DNDGame.MauiApp.ViewModels"
             x:Class="DNDGame.MauiApp.Pages.CharacterListPage"
             Title="Characters">
    
    <ContentPage.BindingContext>
        <vm:CharacterListViewModel />
    </ContentPage.BindingContext>
    
    <Grid RowDefinitions="Auto,*">
        <!-- Header -->
        <StackLayout Padding="20">
            <Label Text="My Characters" FontSize="24" FontAttributes="Bold" />
            <Button Text="Create New Character" Command="{Binding CreateCharacterCommand}" />
        </StackLayout>
        
        <!-- Character List -->
        <CollectionView Grid.Row="1" ItemsSource="{Binding Characters}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <SwipeView>
                        <SwipeView.RightItems>
                            <SwipeItems>
                                <SwipeItem Text="Delete" BackgroundColor="Red" 
                                           Command="{Binding Source={RelativeSource AncestorType={x:Type vm:CharacterListViewModel}}, Path=DeleteCharacterCommand}"
                                           CommandParameter="{Binding .}" />
                            </SwipeItems>
                        </SwipeView.RightItems>
                        
                        <Frame Margin="10" Padding="15" CornerRadius="10">
                            <Grid ColumnDefinitions="Auto,*,Auto">
                                <!-- Character Avatar -->
                                <Image Grid.Column="0" Source="{Binding PortraitUrl}" 
                                       WidthRequest="60" HeightRequest="60" Aspect="AspectFill" />
                                
                                <!-- Character Info -->
                                <StackLayout Grid.Column="1" Padding="10,0">
                                    <Label Text="{Binding Name}" FontSize="18" FontAttributes="Bold" />
                                    <Label Text="{Binding Class}" FontSize="14" TextColor="Gray" />
                                    <Label Text="{Binding Level, StringFormat='Level {0}'}" FontSize="12" />
                                </StackLayout>
                                
                                <!-- Navigation Arrow -->
                                <Image Grid.Column="2" Source="arrow_right.png" WidthRequest="24" />
                            </Grid>
                        </Frame>
                    </SwipeView>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

### Platform-Specific Features

**iOS**:
- Face ID / Touch ID authentication
- Push notifications via APNs
- Haptic feedback on dice rolls

**Android**:
- Fingerprint authentication
- Push notifications via FCM
- Material Design components

**Windows**:
- Windows Hello authentication
- Toast notifications
- Snap layouts support

### Acceptance Criteria
- [ ] App runs on iOS, Android, Windows, macOS
- [ ] Blazor components reused from web app
- [ ] Offline mode works with local SQLite database
- [ ] Push notifications work on all platforms
- [ ] Biometric authentication works
- [ ] Character sync works when online
- [ ] Camera integration for portraits
- [ ] File picker for character import/export
- [ ] Native navigation feels smooth
- [ ] App passes platform store requirements

### Test Results Summary
**Target**: 35+ unit tests

Expected coverage:
- **ViewModels**: 20 tests
  - CharacterListViewModel (8 tests)
  - CharacterDetailViewModel (6 tests)
  - SessionViewModel (6 tests)
- **Services**: 10 tests
  - NotificationService (3 tests)
  - BiometricService (3 tests)
  - OfflineSyncService (4 tests)
- **Platform Tests**: 5 tests
  - iOS-specific features
  - Android-specific features
  - Windows-specific features

**Manual Testing Checklist**:
- [ ] Install on physical iOS device
- [ ] Install on physical Android device
- [ ] Test Windows app
- [ ] Verify offline mode
- [ ] Test push notifications
- [ ] Verify biometric login
- [ ] Test camera integration
- [ ] Check file picker

**Success Criteria**:
- ✅ 100% test pass rate
- ✅ App size <50MB
- ✅ Cold start time <3s
- ✅ Crash-free rate >99%
- ✅ Battery usage <5%/hour

---

## Phase 7: Testing & Polish (Weeks 13-14)

### Goals
Comprehensive testing, performance optimization, security audit, and bug fixes.

### Tasks
- [ ] Write comprehensive unit tests (target: 500+ tests)
- [ ] Add integration tests for API
- [ ] Create Blazor component tests with bUnit
- [ ] Load test SignalR hubs
- [ ] Performance optimization (caching, lazy loading)
- [ ] Security audit (OWASP top 10)
- [ ] Accessibility audit (WCAG 2.1)
- [ ] Code coverage analysis (target: 80%+)
- [ ] Documentation review
- [ ] Bug bash sessions

### Testing Infrastructure
```csharp
// tests/DNDGame.IntegrationTests/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace real database with in-memory database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DndGameContext>));
            
            if (descriptor != null)
                services.Remove(descriptor);
            
            services.AddDbContext<DndGameContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
            
            // Replace real LLM service with mock
            services.AddSingleton<ILlmDmService, MockLlmDmService>();
        });
    }
}

// tests/DNDGame.LoadTests/SignalRLoadTest.cs
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[MemoryDiagnoser]
public class SignalRLoadTest
{
    [Benchmark]
    public async Task Concurrent_Sessions_50_Users()
    {
        var tasks = Enumerable.Range(0, 50)
            .Select(i => ConnectAndPlayAsync(i))
            .ToList();
        
        await Task.WhenAll(tasks);
    }
    
    private async Task ConnectAndPlayAsync(int userId)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:8000/hubs/game-session")
            .Build();
        
        await connection.StartAsync();
        await connection.InvokeAsync("JoinSession", 1, userId);
        
        // Simulate gameplay for 5 minutes
        for (int i = 0; i < 10; i++)
        {
            await connection.InvokeAsync("SendMessage", 1, $"Test message {i}");
            await Task.Delay(30000); // 30 seconds
        }
        
        await connection.StopAsync();
    }
}

// tests/DNDGame.SecurityTests/SecurityTests.cs
public class SecurityTests
{
    [Fact]
    public async Task API_RejectsRequests_WithoutAuthentication()
    {
        var response = await _client.GetAsync("/api/v1/characters");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task API_RejectsRequests_WithExpiredToken()
    {
        var expiredToken = GenerateExpiredToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", expiredToken);
        
        var response = await _client.GetAsync("/api/v1/characters");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task API_PreventsSQLInjection()
    {
        var maliciousInput = "'; DROP TABLE Characters; --";
        var response = await _client.GetAsync($"/api/v1/characters/{maliciousInput}");
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        // Verify table still exists
        var characters = await _context.Characters.ToListAsync();
        Assert.NotNull(characters);
    }
}
```

### Performance Benchmarks
```csharp
// tests/DNDGame.Benchmarks/DiceRollerBenchmarks.cs
[MemoryDiagnoser]
public class DiceRollerBenchmarks
{
    private readonly IDiceRoller _diceRoller = new DiceRollerService();
    
    [Benchmark]
    public void Roll_Single_D20() => _diceRoller.Roll("1d20");
    
    [Benchmark]
    public void Roll_Complex_Formula() => _diceRoller.Roll("8d6+2d8+5");
    
    [Benchmark]
    public void Roll_With_Advantage() => _diceRoller.RollWithAdvantage("1d20+5");
}

// Expected results:
// Roll_Single_D20: <1ms, <100 bytes allocated
// Roll_Complex_Formula: <2ms, <500 bytes allocated
// Roll_With_Advantage: <1.5ms, <150 bytes allocated
```

### Test Coverage Goals
```
DNDGame.Core:           95%+ coverage
DNDGame.Application:    90%+ coverage
DNDGame.Infrastructure: 85%+ coverage
DNDGame.API:           80%+ coverage
DNDGame.Shared:        85%+ coverage
```

### Acceptance Criteria
- [ ] 500+ unit tests passing
- [ ] 80%+ code coverage
- [ ] 50+ integration tests passing
- [ ] 30+ Blazor component tests passing
- [ ] Load tests show <100ms p95 latency
- [ ] Security audit passes (no high/critical issues)
- [ ] Accessibility score >90 (WCAG 2.1 AA)
- [ ] API response time p95 <200ms
- [ ] SignalR latency p95 <100ms
- [ ] Database queries p95 <10ms
- [ ] Zero memory leaks detected
- [ ] Zero SQL injection vulnerabilities

### Test Results Summary
**Target**: 500+ total tests

Expected breakdown:
- **Unit Tests**: 300 tests
  - Core domain: 50 tests
  - Services: 100 tests
  - Rules engine: 60 tests
  - Validators: 40 tests
  - Utilities: 50 tests
- **Integration Tests**: 100 tests
  - API endpoints: 50 tests
  - SignalR hubs: 30 tests
  - Database operations: 20 tests
- **Component Tests**: 50 tests
  - Blazor components: 40 tests
  - MAUI views: 10 tests
- **End-to-End Tests**: 20 tests
  - Full gameplay scenarios: 15 tests
  - User journeys: 5 tests
- **Load Tests**: 10 tests
  - Concurrent users: 5 tests
  - Message throughput: 3 tests
  - Database load: 2 tests
- **Security Tests**: 20 tests
  - Authentication: 5 tests
  - Authorization: 5 tests
  - Input validation: 10 tests

**Performance Benchmarks**:
- API response time: p50=50ms, p95=200ms, p99=500ms
- SignalR latency: p50=30ms, p95=100ms, p99=200ms
- Database queries: p50=2ms, p95=10ms, p99=20ms
- Dice roller: <1ms per roll
- Character creation: <100ms
- Session state sync: <50ms

**Success Criteria**:
- ✅ All tests passing (green)
- ✅ Code coverage targets met
- ✅ Performance within targets
- ✅ Zero security vulnerabilities
- ✅ Accessibility compliant

---

## Phase 8: Deployment (Weeks 15-16)

### Goals
Deploy to production with CI/CD pipelines, monitoring, and documentation.

### Tasks
- [ ] Set up Azure resources (App Service, SQL Database, Redis)
- [ ] Configure CI/CD pipelines (GitHub Actions)
- [ ] Deploy API and databases
- [ ] Publish Blazor Server app
- [ ] Deploy Blazor WebAssembly to CDN
- [ ] Submit MAUI apps to app stores (Apple App Store, Google Play)
- [ ] Set up Application Insights monitoring
- [ ] Configure alerts and dashboards
- [ ] Create deployment documentation
- [ ] Conduct user acceptance testing

### Infrastructure as Code
```yaml
# .github/workflows/deploy-api.yml
name: Deploy API to Azure

on:
  push:
    branches: [ main ]
    paths:
      - 'src/DNDGame.API/**'
      - 'src/DNDGame.Application/**'
      - 'src/DNDGame.Core/**'
      - 'src/DNDGame.Infrastructure/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --logger trx --results-directory TestResults
    
    - name: Publish Test Results
      uses: EnricoMi/publish-unit-test-result-action@v2
      if: always()
      with:
        files: TestResults/**/*.trx
    
    - name: Publish API
      run: dotnet publish src/DNDGame.API/DNDGame.API.csproj --configuration Release --output ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'dndgame-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
    
    - name: Run Database Migrations
      run: |
        dotnet tool install --global dotnet-ef
        dotnet ef database update --project src/DNDGame.Infrastructure --startup-project src/DNDGame.API --connection "${{ secrets.DATABASE_CONNECTION_STRING }}"

# .github/workflows/deploy-blazor.yml
name: Deploy Blazor WebAssembly

on:
  push:
    branches: [ main ]
    paths:
      - 'src/DNDGame.Web.Client/**'
      - 'src/DNDGame.Shared/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Publish Blazor WASM
      run: dotnet publish src/DNDGame.Web.Client/DNDGame.Web.Client.csproj -c Release -o publish
    
    - name: Upload to Azure Static Web Apps
      uses: Azure/static-web-apps-deploy@v1
      with:
        azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
        repo_token: ${{ secrets.GITHUB_TOKEN }}
        action: "upload"
        app_location: "publish/wwwroot"

# .github/workflows/deploy-maui-ios.yml
name: Deploy MAUI iOS to App Store

on:
  push:
    tags:
      - 'v*'

jobs:
  build-ios:
    runs-on: macos-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Install MAUI workloads
      run: dotnet workload install maui
    
    - name: Restore dependencies
      run: dotnet restore src/DNDGame.MauiApp/DNDGame.MauiApp.csproj
    
    - name: Build iOS app
      run: dotnet build src/DNDGame.MauiApp/DNDGame.MauiApp.csproj -c Release -f net8.0-ios
    
    - name: Publish iOS app
      run: dotnet publish src/DNDGame.MauiApp/DNDGame.MauiApp.csproj -c Release -f net8.0-ios -p:ArchiveOnBuild=true
    
    - name: Upload to App Store Connect
      run: |
        xcrun altool --upload-app --type ios --file publish/*.ipa \
          --apiKey ${{ secrets.APP_STORE_API_KEY }} \
          --apiIssuer ${{ secrets.APP_STORE_ISSUER_ID }}
```

### Azure Resources
```bicep
// infrastructure/main.bicep
param location string = 'eastus'
param environment string = 'production'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'dndgame-plan-${environment}'
  location: location
  sku: {
    name: 'P1v2'
    tier: 'PremiumV2'
    capacity: 2
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'dndgame-api-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      webSocketsEnabled: true
    }
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: 'dndgame-sql-${environment}'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: '<secure-password>'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: sqlServer
  name: 'dndgame'
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
}

resource redisCache 'Microsoft.Cache/redis@2023-04-01' = {
  name: 'dndgame-redis-${environment}'
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 1
    }
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'dndgame-appinsights-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}
```

### Monitoring & Alerts
```csharp
// Program.cs - Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});

// Custom metrics
public class MetricsService
{
    private readonly TelemetryClient _telemetry;
    
    public void TrackDiceRoll(string formula, int result)
    {
        _telemetry.TrackEvent("DiceRoll", new Dictionary<string, string>
        {
            ["Formula"] = formula,
            ["Result"] = result.ToString()
        });
    }
    
    public void TrackSessionStart(int sessionId, int playerCount)
    {
        _telemetry.TrackEvent("SessionStart", new Dictionary<string, string>
        {
            ["SessionId"] = sessionId.ToString(),
            ["PlayerCount"] = playerCount.ToString()
        });
    }
    
    public void TrackLlmRequest(string model, int tokens, TimeSpan duration)
    {
        _telemetry.TrackDependency("LLM", model, DateTime.UtcNow, duration, tokens > 0);
        _telemetry.GetMetric("LlmTokensUsed").TrackValue(tokens);
    }
}
```

### Acceptance Criteria
- [ ] API deployed to Azure App Service
- [ ] Database migrations applied successfully
- [ ] Blazor Server app accessible via HTTPS
- [ ] Blazor WASM app deployed to CDN
- [ ] MAUI iOS app submitted to App Store
- [ ] MAUI Android app published to Google Play
- [ ] CI/CD pipelines running successfully
- [ ] Application Insights collecting telemetry
- [ ] Alerts configured for errors and performance
- [ ] Health checks responding correctly
- [ ] SSL certificates configured
- [ ] Custom domain configured (e.g., dndgame.com)

### Test Results Summary
**Deployment Verification Checklist**:

- [ ] **API Health Check**: `GET https://api.dndgame.com/health` returns 200
- [ ] **Database Connectivity**: Connection pool healthy, queries <10ms
- [ ] **Redis Connectivity**: Cache hit rate >80%
- [ ] **SignalR**: WebSocket connections successful
- [ ] **Authentication**: JWT tokens work correctly
- [ ] **LLM Integration**: Responses within 5 seconds
- [ ] **Blazor WASM**: Loads in <3 seconds
- [ ] **MAUI Apps**: Launch time <3 seconds

**Performance Testing (Production)**:
- [ ] Load test: 100 concurrent users for 30 minutes
- [ ] Stress test: 500 concurrent users for 5 minutes
- [ ] Spike test: 1000 users in 10 seconds
- [ ] Endurance test: 50 users for 24 hours

**Monitoring Dashboards**:
- [ ] API response times (p50, p95, p99)
- [ ] SignalR connection count
- [ ] Database query performance
- [ ] LLM API usage and costs
- [ ] Error rates and exceptions
- [ ] User engagement metrics

**Cost Monitoring**:
- Azure App Service: ~$100/month
- Azure SQL Database: ~$30/month
- Azure Cache for Redis: ~$20/month
- Application Insights: ~$10/month
- LLM API (OpenAI): Variable (track per session)
- **Total**: ~$160/month + LLM costs

**Success Criteria**:
- ✅ All services deployed and healthy
- ✅ Zero downtime deployment
- ✅ 99.9% uptime (monthly)
- ✅ <200ms API response time (p95)
- ✅ <100ms SignalR latency (p95)
- ✅ Error rate <0.1%
- ✅ App Store approval received
- ✅ Google Play approval received

---

## Success Metrics

### Week 16 (Post-Deployment)
- [ ] 100+ registered users
- [ ] 50+ active sessions
- [ ] 99.9% uptime
- [ ] <200ms API latency (p95)
- [ ] 4.5+ star rating on app stores
- [ ] <1% error rate
- [ ] <$500/month operational costs

### Month 3
- [ ] 1,000+ registered users
- [ ] 500+ active sessions
- [ ] 10+ concurrent multiplayer sessions
- [ ] Player retention >30%
- [ ] Average session length >30 minutes

---

## Risk Mitigation

### Technical Risks
| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| LLM API costs too high | High | Medium | Implement response caching, use smaller models for simple tasks, set budget alerts |
| SignalR scalability issues | High | Low | Use Azure SignalR Service for horizontal scaling, implement backpressure |
| Database performance | Medium | Low | Add proper indexes, implement read replicas, use Redis caching |
| MAUI app store rejection | Medium | Medium | Follow platform guidelines strictly, test on physical devices, prepare appeal documentation |
| Real-time sync conflicts | Medium | Medium | Implement conflict resolution strategies, use optimistic concurrency |

### Product Risks
| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| DM responses poor quality | High | Medium | Iterate on prompts, add examples, implement feedback loop |
| User confusion with rules | Medium | High | Add in-game tutorial, tooltips, comprehensive help system |
| Low user engagement | High | Low | Implement achievements, leaderboards, social features |
| Security vulnerabilities | High | Low | Regular security audits, penetration testing, bug bounty program |

---

## Future Enhancements (Beyond Phase 8)

### Advanced Features
- [ ] Voice mode (speech-to-text, text-to-speech)
- [ ] AI-generated character portraits
- [ ] Custom homebrew content creation
- [ ] Campaign templates marketplace
- [ ] Party voice chat integration
- [ ] Augmented reality dice roller
- [ ] Virtual tabletop integration
- [ ] Twitch/YouTube streaming integration

### Scalability
- [ ] Kubernetes deployment
- [ ] Multi-region support
- [ ] Database sharding
- [ ] CDN for global reach
- [ ] WebAssembly performance optimization
- [ ] Server-side Blazor circuit optimization

### Monetization
- [ ] Premium subscription tier
- [ ] In-app purchases (cosmetics, portraits)
- [ ] Campaign marketplace
- [ ] Ad-supported free tier
- [ ] Affiliate program

---

## Summary

This 16-week roadmap transforms the DNDGame concept into a production-ready .NET application:

- **Weeks 1-2**: Foundation (models, database, basic API)
- **Weeks 3-4**: Game Logic (D&D rules engine)
- **Weeks 5-6**: LLM Integration (AI Dungeon Master)
- **Weeks 7-8**: Real-Time Features (SignalR multiplayer)
- **Weeks 9-10**: Blazor Web UI (beautiful components)
- **Weeks 11-12**: MAUI Mobile App (cross-platform native)
- **Weeks 13-14**: Testing & Polish (500+ tests, 80%+ coverage)
- **Weeks 15-16**: Deployment (Azure, CI/CD, app stores)

**Total**: 500+ tests, 80%+ code coverage, production-ready deployment with monitoring and CI/CD.

**Ready to start building the future of AI-powered tabletop RPGs!** 🎲🐉
