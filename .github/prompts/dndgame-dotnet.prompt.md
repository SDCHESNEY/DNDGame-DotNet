---
mode: 'agent'
description: 'Collection of reusable prompts for .NET Core, Blazor, and MAUI development tasks in the DNDGame project'
tools: ['changes', 'codebase', 'edit/editFiles', 'fetch', 'githubRepo', 'problems', 'runCommands', 'runTasks', 'search', 'usages']
---

# DNDGame .NET Prompts Collection

This file contains ready-to-use prompts for common development scenarios in the DNDGame project.

## Table of Contents

1. [API Development Prompts](#api-development-prompts)
2. [Blazor Component Prompts](#blazor-component-prompts)
3. [MAUI Mobile Prompts](#maui-mobile-prompts)
4. [SignalR Real-Time Prompts](#signalr-real-time-prompts)
5. [Entity Framework Prompts](#entity-framework-prompts)
6. [Testing Prompts](#testing-prompts)
7. [Game Mechanics Prompts](#game-mechanics-prompts)

---

## API Development Prompts

### Create REST API Endpoint

**Usage**: `/create-api-endpoint`

Create a new REST API endpoint for [entity name] with the following requirements:
- HTTP method: [GET/POST/PUT/DELETE]
- Route: `/api/v1/[resource]`
- Request model: [description of input]
- Response model: [description of output]
- Include input validation using FluentValidation
- Implement proper error handling with Problem Details
- Add XML documentation for OpenAPI
- Return appropriate HTTP status codes

Example: "Create a REST API endpoint to get all characters for a player"

---

### Add JWT Authentication

**Usage**: `/add-jwt-auth`

Implement JWT bearer authentication for the API with:
- Token generation on login
- Token validation middleware
- User claims (UserId, Email, Roles)
- Refresh token support
- Configure token lifetime and secret key from appsettings
- Add [Authorize] attributes to protected endpoints

---

### Create Minimal API

**Usage**: `/create-minimal-api`

Create a minimal API endpoint group for [entity name] with:
- Map endpoints for CRUD operations
- Use endpoint filters for validation
- Configure route groups with versioning
- Add OpenAPI metadata with WithOpenApi()
- Implement dependency injection for services

---

### Add API Versioning

**Usage**: `/add-api-versioning`

Implement API versioning with:
- URL segment versioning: `/api/v1/`, `/api/v2/`
- Configure Asp.Versioning.Http NuGet package
- Add version info to OpenAPI documentation
- Support deprecation warnings
- Provide migration guide in comments

---

## Blazor Component Prompts

### Create Blazor Component

**Usage**: `/create-blazor-component`

Create a Blazor component named [ComponentName] with:
- Component parameters for [list parameters]
- Event callbacks for [list events]
- Component lifecycle methods (OnInitializedAsync, OnParametersSetAsync)
- Data binding with two-way binding where needed
- Code-behind file (.razor.cs) for complex logic
- CSS isolation file (.razor.css) for styling
- Proper disposal implementation if needed

Example: "Create a Blazor component for displaying a character's ability scores"

---

### Add Form Validation

**Usage**: `/add-blazor-form-validation`

Add validation to a Blazor EditForm for [model name]:
- Use DataAnnotationsValidator
- Add ValidationSummary for overall errors
- Include ValidationMessage for individual fields
- Implement custom validation attributes if needed
- Handle validation on submit
- Display user-friendly error messages
- Style validation states with CSS

---

### Create State Container

**Usage**: `/create-state-container`

Create a state container service for [state name]:
- Implement INotifyPropertyChanged or custom event pattern
- Register as scoped or singleton in DI
- Provide methods to update state
- Notify subscribers of changes
- Use in components via @inject directive
- Include StateHasChanged() calls where needed

---

### Add SignalR to Blazor

**Usage**: `/add-signalr-to-blazor`

Integrate SignalR in a Blazor component for [feature name]:
- Create HubConnection with automatic reconnect
- Subscribe to hub methods in OnInitializedAsync
- Implement connection state handling
- Call StateHasChanged() when receiving messages
- Dispose connection properly
- Handle connection errors gracefully

---

## MAUI Mobile Prompts

### Create MAUI ViewModel

**Usage**: `/create-maui-viewmodel`

Create a MAUI ViewModel for [ViewName] with:
- Inherit from ObservableObject (CommunityToolkit.Mvvm)
- Use [ObservableProperty] for bindable properties
- Implement [RelayCommand] for UI actions
- Add async command methods with cancellation tokens
- Include loading states and error handling
- Inject required services via constructor
- Follow MVVM pattern strictly

Example: "Create a MAUI ViewModel for the character list page"

---

### Create MAUI View

**Usage**: `/create-maui-view`

Create a MAUI ContentPage for [ViewName]:
- Design XAML layout with proper data bindings
- Use CollectionView for lists
- Implement pull-to-refresh if applicable
- Add loading indicators (ActivityIndicator)
- Display error messages with appropriate styling
- Bind ViewModel commands to UI elements
- Support platform-specific styling

---

### Add MAUI Navigation

**Usage**: `/add-maui-navigation`

Implement Shell navigation in MAUI:
- Define routes in AppShell.xaml
- Register routes with Routing.RegisterRoute()
- Navigate using Shell.Current.GoToAsync()
- Pass parameters via query strings or navigation parameters
- Handle navigation events (OnNavigatedTo, OnNavigatedFrom)
- Implement back button handling

---

### Consume API in MAUI

**Usage**: `/consume-api-maui`

Create a service to consume REST API in MAUI:
- Use HttpClient with IHttpClientFactory
- Implement CRUD operations for [entity]
- Add authentication headers (Bearer token)
- Handle network errors gracefully
- Implement retry logic with Polly
- Use JSON serialization options
- Cache responses when appropriate

---

## SignalR Real-Time Prompts

### Create SignalR Hub

**Usage**: `/create-signalr-hub`

Create a SignalR hub for [feature name]:
- Define hub methods for [list methods]
- Implement group-based messaging
- Add authentication requirements
- Handle connection lifetime events
- Use strongly-typed hub with interface
- Include error handling and logging
- Document hub methods with XML comments

Example: "Create a SignalR hub for managing game sessions"

---

### Add SignalR Client

**Usage**: `/add-signalr-client`

Implement SignalR client functionality:
- Create HubConnection with configuration
- Subscribe to server methods
- Implement send methods for [actions]
- Handle reconnection with exponential backoff
- Manage connection state (Connected, Reconnecting, Disconnected)
- Display connection status to users
- Clean up on disposal

---

### Test SignalR Hub

**Usage**: `/test-signalr-hub`

Create tests for SignalR hub [HubName]:
- Set up test server with WebApplicationFactory
- Create test client connections
- Test hub methods with various inputs
- Verify messages sent to clients
- Test group membership and isolation
- Test connection lifecycle
- Use xUnit and FluentAssertions

---

## Entity Framework Prompts

### Create Entity Model

**Usage**: `/create-entity-model`

Create an Entity Framework entity for [entity name]:
- Define properties with appropriate types
- Add data annotations or Fluent API configuration
- Configure relationships (one-to-many, many-to-many)
- Add navigation properties
- Include audit fields (CreatedAt, UpdatedAt)
- Configure indexes for performance
- Add value converters for enums

---

### Create Repository

**Usage**: `/create-repository`

Create a repository for [entity name]:
- Define IRepository interface
- Implement Repository class
- Include CRUD operations
- Add query methods with filtering
- Implement pagination support
- Use AsNoTracking() for read-only queries
- Add unit of work pattern if needed

---

### Create Migration

**Usage**: `/create-migration`

Create an Entity Framework migration:
- Add new entity or modify existing
- Generate migration with descriptive name
- Review Up() and Down() methods
- Test migration on development database
- Document breaking changes
- Seed initial data if needed

---

### Optimize EF Query

**Usage**: `/optimize-ef-query`

Optimize Entity Framework query for [scenario]:
- Use Include() and ThenInclude() for eager loading
- Implement Select() projections to reduce data
- Apply AsNoTracking() for read-only queries
- Use compiled queries for hot paths
- Add appropriate indexes
- Split complex queries if needed
- Measure performance before and after

---

## Testing Prompts

### Create Unit Tests

**Usage**: `/create-unit-tests`

Create unit tests for [class name]:
- Use xUnit test framework
- Follow Arrange-Act-Assert pattern
- Use FluentAssertions for assertions
- Mock dependencies with Moq
- Test happy path and edge cases
- Use [Theory] and [InlineData] for parameterized tests
- Aim for 80%+ code coverage

---

### Create Integration Tests

**Usage**: `/create-integration-tests`

Create integration tests for [feature name]:
- Use WebApplicationFactory for API tests
- Set up test database (in-memory or test container)
- Test full request/response cycle
- Verify database state after operations
- Test authentication and authorization
- Clean up test data after each test

---

### Test Blazor Component

**Usage**: `/test-blazor-component`

Create bUnit tests for Blazor component [ComponentName]:
- Set up test context
- Render component with parameters
- Verify rendered markup
- Test user interactions (clicks, input)
- Verify event callbacks are invoked
- Test component lifecycle
- Mock injected services

---

## Game Mechanics Prompts

### Implement Dice Rolling

**Usage**: `/implement-dice-rolling`

Create a dice rolling system:
- Parse dice notation (e.g., "2d20+5", "1d6")
- Support standard dice (d4, d6, d8, d10, d12, d20, d100)
- Implement advantage/disadvantage
- Return detailed results (individual rolls, modifiers, total)
- Log rolls for audit trail
- Use cryptographically secure RNG
- Add validation for invalid formulas

---

### Create Character Sheet

**Usage**: `/create-character-sheet`

Implement a character sheet system:
- Model D&D 5e character attributes
- Calculate ability modifiers: (score - 10) / 2
- Compute proficiency bonus by level
- Track hit points, armor class, speed
- Manage skills and proficiencies
- Handle spell slots for casters
- Support inventory and equipment
- Implement character progression

---

### Implement Combat System

**Usage**: `/implement-combat-system`

Create a turn-based combat system:
- Roll initiative for combatants
- Sort and track turn order
- Allow actions (attack, cast spell, dodge, etc.)
- Calculate attack rolls with modifiers
- Apply damage and track HP
- Handle conditions (stunned, prone, etc.)
- Support reactions and opportunity attacks
- End combat when appropriate

---

### Create Session Manager

**Usage**: `/create-session-manager`

Build a game session management system:
- Create and configure sessions
- Track session state (created, active, paused, completed)
- Manage player roster and character assignments
- Save and restore session snapshots
- Track combat encounters
- Log major events and rolls
- Handle session timeouts
- Support multiplayer synchronization

---

### Implement LLM Integration

**Usage**: `/implement-llm-integration`

Create LLM Dungeon Master integration:
- Design prompt templates for scene generation
- Call LLM API with proper configuration
- Parse LLM responses into structured data
- Maintain conversation context and memory
- Implement retry logic with exponential backoff
- Add content moderation filters
- Handle rate limiting
- Log LLM interactions for debugging

---

### Create Ability Check System

**Usage**: `/create-ability-check-system`

Implement ability and skill checks:
- Define all D&D 5e abilities (STR, DEX, CON, INT, WIS, CHA)
- Map skills to abilities
- Roll 1d20 + ability modifier + proficiency (if applicable)
- Support advantage/disadvantage
- Compare against Difficulty Class (DC)
- Return success/failure with details
- Log checks for transparency

---

### Implement Spell Casting

**Usage**: `/implement-spell-casting`

Create a spell casting system:
- Model spell properties (level, school, components, range, duration)
- Track spell slots by character level
- Implement spell preparation for prepared casters
- Handle concentration checks
- Support cantrips (0-level spells)
- Apply spell effects (damage, healing, buffs)
- Implement saving throws
- Track active spell effects

---

### Create Inventory System

**Usage**: `/create-inventory-system`

Build a character inventory system:
- Model items with properties (name, weight, value, rarity)
- Track equipped items (armor, weapons, accessories)
- Calculate encumbrance (carrying capacity)
- Support item stacking for consumables
- Implement equipment bonuses (AC, attack, damage)
- Allow item trading between players
- Track currency (copper, silver, gold, platinum)
- Support magic items with special properties

---

## Usage Instructions

To use these prompts in GitHub Copilot Chat:

1. **Direct Copy**: Copy the prompt description and paste into Copilot Chat
2. **With Context**: Add specific details relevant to your feature
3. **Iterative**: Start with the prompt, then refine based on Copilot's response
4. **Combine**: Mix multiple prompts for complex features

### Example Usage:

```
User: /create-blazor-component

Create a Blazor component named CharacterAbilityScores with:
- Component parameters for Character object
- Display all six ability scores (STR, DEX, CON, INT, WIS, CHA)
- Show ability modifiers calculated as (score - 10) / 2
- Allow editing scores with validation (3-20 range)
- Event callback when scores change
- Responsive grid layout
```

### Tips for Better Results:

- **Be Specific**: Include entity names, properties, and requirements
- **Provide Context**: Mention related classes or components
- **Set Constraints**: Specify validation rules, ranges, formats
- **Request Examples**: Ask for sample data or usage examples
- **Ask for Tests**: Request unit tests alongside implementation

---

## Custom Prompt Template

When creating your own prompts, follow this template:

```markdown
### [Prompt Title]

**Usage**: `/your-prompt-name`

[Brief description of what the prompt does]

Requirements:
- [Requirement 1]
- [Requirement 2]
- [Requirement 3]

Expected Output:
- [Output 1]
- [Output 2]

Example: "[Concrete example of usage]"
```

---

## Additional Resources

- [DNDGame Architecture](../copilot-instructions.md) - Project coding standards
- [DNDGame Expert Agent](../agents/dndgame-dotnet-expert.agent.md) - Specialized AI assistant
- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [SignalR Documentation](https://learn.microsoft.com/aspnet/core/signalr/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
