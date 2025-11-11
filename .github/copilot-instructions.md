---
description: 'Comprehensive coding standards and best practices for .NET Core, Blazor, and MAUI development in the DNDGame project'
applyTo: '**/*.cs,**/*.razor,**/*.xaml,**/*.csproj'
---

# DNDGame .NET Core, Blazor & MAUI Development Guidelines

## Project Overview

This is a Dungeons & Dragons-style RPG game with:
- **Backend**: .NET Core API with FastAPI-inspired architecture
- **Web Frontend**: Blazor Server/WebAssembly for rich UI
- **Mobile**: .NET MAUI for cross-platform mobile experience
- **Real-time**: SignalR for WebSocket communication
- **Data**: Entity Framework Core with SQL Server/SQLite

## C# Language Standards

### Modern C# Features (C# 12+)
- Use file-scoped namespaces: `namespace DNDGame.Core;`
- Prefer primary constructors for simple classes
- Use collection expressions: `List<string> items = [];`
- Leverage pattern matching and switch expressions
- Use `required` keyword for mandatory properties
- Apply nullable reference types throughout (`#nullable enable`)

### Code Style
- Follow .NET naming conventions (PascalCase for public, camelCase for private)
- Use expression-bodied members for simple properties and methods
- Prefer `async/await` for all I/O operations
- Use `ConfigureAwait(false)` in library code
- Apply `nameof()` instead of string literals
- Include XML documentation for public APIs with `<example>` blocks

### Example Code Style
```csharp
namespace DNDGame.Core.Characters;

public class Character
{
    public required string Name { get; init; }
    public required int Level { get; init; }
    public CharacterClass Class { get; init; }
    
    public int CalculateHitPoints() => Level * 10 + GetConstitutionBonus();
    
    private int GetConstitutionBonus() => (Constitution - 10) / 2;
}
```

## .NET Core API Architecture

### Project Structure
```
DNDGame.API/
├── Controllers/          # API endpoints
├── Services/            # Business logic
├── Models/              # DTOs and view models
├── Data/                # EF Core context and repositories
├── SignalR/             # Real-time hubs
└── Middleware/          # Custom middleware
```

### Dependency Injection
- Register services in `Program.cs` with appropriate lifetimes
- Use constructor injection exclusively
- Avoid service locator pattern
- Register interfaces, not concrete types

```csharp
// Good
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddSingleton<IDiceRoller, CryptoDiceRoller>();

// Bad - Don't do this
builder.Services.AddScoped<CharacterService>();
```

### API Design
- Follow REST principles for HTTP endpoints
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
- Return proper status codes (200, 201, 400, 404, 500)
- Implement Problem Details (RFC 7807) for errors
- Version APIs using URL segments: `/api/v1/characters`

### Error Handling
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred",
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}
```

## Blazor Development

### Component Structure
- Use code-behind (`.razor.cs`) for complex logic
- Keep `.razor` files focused on markup and simple bindings
- Implement `IDisposable` for cleanup
- Use `OnInitializedAsync` for async initialization

### Component Example
```razor
@page "/character/{CharacterId:int}"
@inject ICharacterService CharacterService

<h3>@character?.Name</h3>

@if (character is not null)
{
    <div class="character-sheet">
        <CharacterStats Character="character" />
        <InventoryPanel Items="character.Inventory" />
    </div>
}

@code {
    [Parameter]
    public int CharacterId { get; set; }
    
    private Character? character;
    
    protected override async Task OnInitializedAsync()
    {
        character = await CharacterService.GetCharacterAsync(CharacterId);
    }
}
```

### State Management
- Use Blazor's built-in state management for simple scenarios
- Implement `INotifyPropertyChanged` or observable patterns for complex state
- Consider Fluxor or similar for large applications
- Use `StateHasChanged()` judiciously to trigger re-renders

### Event Handling
- Use `EventCallback<T>` for component events
- Implement debouncing for rapid-fire events (search, slider)
- Use `@onclick:preventDefault` and `@onclick:stopPropagation` when needed

### Performance
- Virtualize large lists with `<Virtualize>` component
- Use `@key` directive for dynamic lists
- Implement `ShouldRender()` for expensive components
- Prerender Blazor Server apps for faster initial load

## .NET MAUI Development

### MVVM Pattern
- Separate UI (XAML) from logic (ViewModels)
- Use `CommunityToolkit.Mvvm` for `ObservableObject` and `RelayCommand`
- Bind to ViewModels, never code-behind
- Keep ViewModels platform-agnostic

### ViewModel Example
```csharp
public partial class CharacterViewModel : ObservableObject
{
    private readonly ICharacterService _characterService;
    
    [ObservableProperty]
    private Character? selectedCharacter;
    
    [ObservableProperty]
    private ObservableCollection<Character> characters = [];
    
    public CharacterViewModel(ICharacterService characterService)
    {
        _characterService = characterService;
    }
    
    [RelayCommand]
    private async Task LoadCharactersAsync()
    {
        var chars = await _characterService.GetAllCharactersAsync();
        Characters = new ObservableCollection<Character>(chars);
    }
    
    [RelayCommand]
    private async Task SelectCharacterAsync(Character character)
    {
        SelectedCharacter = character;
        await Shell.Current.GoToAsync($"character-detail?id={character.Id}");
    }
}
```

### XAML Best Practices
- Use data binding exclusively (`{Binding}`, `{x:Bind}`)
- Leverage `Style` and `ResourceDictionary` for consistent UI
- Use platform-specific code sparingly (prefer conditional compilation)
- Implement accessibility features (AutomationProperties)

### Navigation
- Use Shell navigation for consistent experience
- Define routes in `AppShell.xaml`
- Pass parameters via query strings or navigation parameters
- Implement deep linking for notifications

## Entity Framework Core

### DbContext Configuration
```csharp
public class DndGameContext : DbContext
{
    public DndGameContext(DbContextOptions<DndGameContext> options)
        : base(options) { }
    
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Message> Messages => Set<Message>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DndGameContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Entity Configuration
- Use Fluent API over Data Annotations for complex configurations
- Create separate configuration classes (IEntityTypeConfiguration)
- Define relationships explicitly
- Configure value converters for enums and custom types

### Queries
- Use `AsNoTracking()` for read-only queries
- Implement pagination with `Skip()` and `Take()`
- Use projections (`.Select()`) to return only needed data
- Avoid N+1 queries with `.Include()` and `.ThenInclude()`

## SignalR for Real-Time Features

### Hub Implementation
```csharp
public class GameHub : Hub
{
    private readonly ISessionService _sessionService;
    
    public GameHub(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }
    
    public async Task JoinSession(int sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        await Clients.Group($"session-{sessionId}").SendAsync("PlayerJoined", Context.ConnectionId);
    }
    
    public async Task SendMessage(int sessionId, string message)
    {
        var saved = await _sessionService.SaveMessageAsync(sessionId, message);
        await Clients.Group($"session-{sessionId}").SendAsync("ReceiveMessage", saved);
    }
    
    public async Task RollDice(int sessionId, string formula)
    {
        var result = await _sessionService.RollDiceAsync(sessionId, formula);
        await Clients.Group($"session-{sessionId}").SendAsync("DiceRolled", result);
    }
}
```

### Client Configuration (Blazor)
```csharp
private HubConnection? hubConnection;

protected override async Task OnInitializedAsync()
{
    hubConnection = new HubConnectionBuilder()
        .WithUrl(Navigation.ToAbsoluteUri("/gamehub"))
        .WithAutomaticReconnect()
        .Build();
    
    hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
    {
        messages.Add(new Message { User = user, Content = message });
        StateHasChanged();
    });
    
    await hubConnection.StartAsync();
}
```

## Testing Standards

### Unit Testing
- Use xUnit as the primary testing framework
- Follow Arrange-Act-Assert pattern (no comments needed)
- Use FluentAssertions for readable assertions
- Mock dependencies with Moq or NSubstitute

```csharp
public class CharacterServiceTests
{
    private readonly Mock<ICharacterRepository> _mockRepo;
    private readonly CharacterService _sut;
    
    public CharacterServiceTests()
    {
        _mockRepo = new Mock<ICharacterRepository>();
        _sut = new CharacterService(_mockRepo.Object);
    }
    
    [Fact]
    public async Task GetCharacterAsync_WithValidId_ReturnsCharacter()
    {
        var expected = new Character { Id = 1, Name = "Gandalf" };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expected);
        
        var result = await _sut.GetCharacterAsync(1);
        
        result.Should().BeEquivalentTo(expected);
    }
}
```

### Integration Testing
- Use WebApplicationFactory for API testing
- Test actual database interactions with test containers
- Verify SignalR hub interactions
- Test authentication and authorization flows

## Security Best Practices

### Authentication & Authorization
- Implement JWT bearer authentication for API
- Use ASP.NET Core Identity for user management
- Apply `[Authorize]` attributes at controller/action level
- Implement role-based and policy-based authorization

### Data Protection
- Never log sensitive information (passwords, tokens)
- Use Data Protection API for encrypting sensitive data
- Implement proper CORS policies
- Validate and sanitize all user inputs

### API Security
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
        };
    });
```

## Performance Optimization

### API Performance
- Implement response caching for static content
- Use output caching for expensive operations
- Implement rate limiting to prevent abuse
- Use `IMemoryCache` for frequently accessed data
- Consider Redis for distributed caching

### Database Performance
- Create indexes on frequently queried columns
- Use compiled queries for hot paths
- Implement connection pooling
- Use database pagination instead of in-memory
- Profile queries with EF Core logging

### Blazor Performance
- Minimize component re-renders
- Use streaming rendering for slow data
- Implement lazy loading for routes
- Compress static assets
- Use CDN for static content

## Logging and Monitoring

### Structured Logging
```csharp
public class CharacterService : ICharacterService
{
    private readonly ILogger<CharacterService> _logger;
    
    public CharacterService(ILogger<CharacterService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Character?> GetCharacterAsync(int id)
    {
        _logger.LogInformation("Fetching character with ID {CharacterId}", id);
        
        try
        {
            var character = await _repository.GetByIdAsync(id);
            
            if (character is null)
            {
                _logger.LogWarning("Character with ID {CharacterId} not found", id);
            }
            
            return character;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching character with ID {CharacterId}", id);
            throw;
        }
    }
}
```

### Application Insights
- Integrate Application Insights for telemetry
- Track custom events for game actions
- Monitor API response times
- Set up alerts for errors and performance issues

## Configuration Management

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DndGame;..."
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "dndgame-api",
    "Audience": "dndgame-clients"
  },
  "GameSettings": {
    "MaxPlayersPerSession": 6,
    "SessionTimeoutMinutes": 30,
    "DiceRollHistoryLimit": 100
  }
}
```

### Environment-Specific Settings
- Use `appsettings.Development.json` for local development
- Use User Secrets for sensitive data in development
- Use Azure Key Vault or AWS Secrets Manager in production
- Never commit secrets to version control

## Git and Version Control

### Branch Strategy
- `main` - production-ready code
- `develop` - integration branch
- `feature/*` - new features
- `bugfix/*` - bug fixes
- `hotfix/*` - urgent production fixes

### Commit Messages
```
feat: Add character creation API endpoint
fix: Resolve dice roll calculation bug
docs: Update API documentation
test: Add integration tests for session service
refactor: Extract dice rolling logic to separate service
```

## Documentation Requirements

### XML Documentation
- Document all public APIs with `<summary>`
- Include `<param>` and `<returns>` tags
- Add `<example>` blocks for complex methods
- Document exceptions with `<exception>`

### Code Comments
- Explain "why" not "what"
- Document business rules and domain logic
- Add TODO comments with ticket references
- Remove commented-out code before committing

## Game-Specific Patterns

### Dice Rolling System
```csharp
public interface IDiceRoller
{
    DiceRollResult Roll(string formula); // e.g., "2d20+5"
    Task<DiceRollResult> RollAsync(string formula);
}

public record DiceRollResult(
    string Formula,
    int Total,
    int[] IndividualRolls,
    int Modifier,
    DateTime Timestamp);
```

### Character State Management
- Use immutable records for character snapshots
- Implement event sourcing for character progression
- Store historical data for character development
- Validate state transitions

### Session Lifecycle
```csharp
public enum SessionState
{
    Created,
    WaitingForPlayers,
    InProgress,
    Paused,
    Completed,
    Abandoned
}
```

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr/)
- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
