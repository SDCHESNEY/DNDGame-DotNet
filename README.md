# DNDGame - LLM-Powered Dungeon Master

A Dungeons & Dragons-style RPG powered by Large Language Models, built with .NET 9.0, Blazor, and .NET MAUI.

[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Tests](https://img.shields.io/badge/tests-174%20passing-success)](tests/)

## 🎮 Overview

DNDGame is an innovative RPG that combines classic Dungeons & Dragons gameplay with modern AI technology. An LLM acts as your Dungeon Master, creating dynamic narratives, managing game state, and responding to player actions in real-time.

### Key Features

- **AI Dungeon Master**: OpenAI-powered narrative generation with context-aware responses ✅
- **Streaming Responses**: Server-Sent Events for real-time DM responses ✅
- **Content Moderation**: Automatic filtering of inappropriate content ✅
- **D&D 5e Rules**: Authentic ability scores, proficiency bonuses, and character progression ✅
- **Combat & Exploration**: Context-aware prompts adapt to game mode ✅
- **NPC Dialogue**: Dynamic conversations with personality-driven NPCs ✅
- **Cross-Platform**: 
  - Web application (Blazor Server/WebAssembly)
  - Mobile apps (iOS/Android via .NET MAUI)
  - REST API for third-party integrations ✅
- **Real-Time Gameplay**: SignalR-powered WebSocket communication (Phase 4)
- **Persistent World**: SQLite/PostgreSQL database with Entity Framework Core ✅
- **Character Management**: Full CRUD operations for D&D characters ✅
- **Session System**: Solo and multiplayer campaign support ✅

## 🏗️ Architecture

Built using Clean Architecture principles with clear separation of concerns:

```
DNDGame-DotNet/
├── src/
│   ├── DNDGame.Core/           # Domain entities, interfaces, value objects
│   ├── DNDGame.Infrastructure/ # EF Core, repositories, external services
│   ├── DNDGame.Application/    # Business logic, DTOs, validators
│   ├── DNDGame.API/            # REST API controllers, middleware
│   ├── DNDGame.Web/            # Blazor web application (planned)
│   └── DNDGame.MAUI/           # Mobile application (planned)
├── tests/
│   └── DNDGame.UnitTests/      # xUnit tests (48 passing)
└── docs/                       # Documentation and roadmap
```

### Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | .NET 9.0 |
| **Web API** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 9.0 |
| **Database** | SQLite (development) / PostgreSQL (production) |
| **Validation** | FluentValidation 12.1 |
| **Web UI** | Blazor Server/WebAssembly (Phase 5) |
| **Mobile** | .NET MAUI (Phase 6) |
| **Real-Time** | SignalR (Phase 4) |
| **Testing** | xUnit, Moq 4.20.72, FluentAssertions 8.8.0 |
| **AI Integration** | OpenAI SDK 2.6.0, Polly 8.6.4 ✅ |

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQLite (included) or [PostgreSQL](https://www.postgresql.org/) for production
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- Optional: [OpenAI API Key](https://platform.openai.com/) for LLM features
- Optional: [dotnet-ef tools](https://learn.microsoft.com/ef/core/cli/dotnet) for migrations

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SDCHESNEY/DNDGame-DotNet.git
   cd DNDGame-DotNet
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Apply database migrations**
   
   The project uses SQLite for development (database file created automatically):
   ```bash
   dotnet ef database update --project src/DNDGame.Infrastructure --startup-project src/DNDGame.API
   ```
   
   Or if using dotnet-ef tools:
   ```bash
   export PATH="$PATH:$HOME/.dotnet/tools"
   dotnet ef database update --project src/DNDGame.Infrastructure --startup-project src/DNDGame.API
   ```
   
   The database file will be created at `src/DNDGame.API/dndgame.db`

4. **Run the API**
   ```bash
   cd src/DNDGame.API
   dotnet run
   ```

5. **Configure AI Features (Optional)**
   
   Add your OpenAI API key to `appsettings.Development.json`:
   ```json
   {
     "LLM": {
       "Provider": "OpenAI",
       "ApiKey": "sk-your-api-key-here",
       "Model": "gpt-4-turbo-preview",
       "MaxTokens": 500,
       "Temperature": 0.7
     },
     "ContentModeration": {
       "Enabled": true,
       "BlockNSFW": true,
       "BlockHarassment": true,
       "MaxInputLength": 2000
     }
   }
   ```
   
   Or set via environment variable:
   ```bash
   export LLM__ApiKey="sk-your-api-key-here"
   ```

6. **Access the API**
   - API: `https://localhost:5001`
   - OpenAPI/Swagger: `https://localhost:5001/openapi/v1.json`

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

Current test status: **174/174 passing** ✅

Test breakdown:
- **Domain Models**: 15 tests (ability scores, character, session)
- **Game Logic Services**: 64 tests (dice roller, rules engine, combat)
- **Application Services**: 14 tests (character service, session service)
- **Validators**: 37 tests (character, session request validation)
- **LLM Services**: 40 tests (prompt templates, content moderation, DM responses)
- **Integration Tests**: 10 tests (end-to-end API with database)

## 📚 API Documentation

### Character Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/characters/{id}` | Get character by ID |
| `GET` | `/api/v1/characters/player/{playerId}` | Get all characters for a player |
| `POST` | `/api/v1/characters/player/{playerId}` | Create new character |
| `PUT` | `/api/v1/characters/{id}` | Update character |
| `DELETE` | `/api/v1/characters/{id}` | Delete character |

### Session Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/sessions/{id}` | Get session by ID |
| `GET` | `/api/v1/sessions` | Get all sessions |
| `POST` | `/api/v1/sessions` | Create new session |
| `PATCH` | `/api/v1/sessions/{id}/state` | Update session state |
| `DELETE` | `/api/v1/sessions/{id}` | Delete session |

### Dice & Combat Endpoints (Phase 2)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/dice/roll` | Roll dice with formula (1d20+5) |
| `POST` | `/api/v1/combat/{sessionId}/initiative` | Roll initiative for session |
| `POST` | `/api/v1/combat/attack` | Resolve attack between characters |
| `POST` | `/api/v1/combat/{characterId}/damage` | Apply damage to character |
| `POST` | `/api/v1/combat/{characterId}/heal` | Apply healing to character |

### AI Dungeon Master Endpoints (Phase 3) ✅

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/v1/dm/generate` | Generate DM response with context |
| `POST` | `/api/v1/dm/stream` | Stream DM response via SSE |
| `POST` | `/api/v1/dm/npc` | Generate NPC dialogue |
| `POST` | `/api/v1/dm/scene` | Generate scene description |

### Example: Create Character

```bash
curl -X POST https://localhost:5001/api/v1/characters/player/1 \
  -H "Content-Type: application/json" \
  -d '{
    "playerId": 1,
    "name": "Gandalf the Grey",
    "class": "Wizard",
    "level": 5,
    "abilityScores": {
      "strength": 10,
      "dexterity": 14,
      "constitution": 12,
      "intelligence": 18,
      "wisdom": 16,
      "charisma": 12
    },
    "maxHitPoints": 30,
    "armorClass": 12
  }'
```

### Example: Generate AI DM Response

```bash
curl -X POST https://localhost:5001/api/v1/dm/generate \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": 1,
    "playerAction": "I search the ancient library for clues",
    "contextMessages": 10
  }'
```

Response:
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

### Example: Stream DM Response (SSE)

```bash
curl -X POST https://localhost:5001/api/v1/dm/stream \
  -H "Content-Type: application/json" \
  -H "Accept: text/event-stream" \
  -d '{
    "sessionId": 1,
    "playerAction": "I cast Fireball at the goblin horde"
  }'
```

Streaming output:
```
data: The 
data: flames 
data: erupt 
data: from 
data: your 
data: fingertips...
[DONE]
```

## 🎯 Domain Models

### Character
- **Attributes**: Name, Class, Level, Ability Scores, Hit Points, Armor Class
- **Calculated**: Proficiency Bonus (D&D 5e formula: `2 + (Level - 1) / 4`)
- **Collections**: Skills, Inventory
- **Relationships**: Belongs to Player

### Session
- **Modes**: Solo, Multiplayer
- **States**: Created, WaitingForPlayers, InProgress, Paused, Completed, Abandoned
- **Features**: Turn tracking, scene management, world state (JSON)
- **Relationships**: Contains Messages and DiceRolls

### AbilityScores (Value Object)
```csharp
// Immutable record with automatic modifier calculation
public record AbilityScores(
    int Strength,
    int Dexterity,
    int Constitution,
    int Intelligence,
    int Wisdom,
    int Charisma)
{
    public int StrengthModifier => GetModifier(Strength);
    // ... (D&D 5e formula: (score - 10) / 2)
}
```

## 🧪 Testing

The project includes comprehensive unit tests covering:

- **Domain Logic** (15 tests): Entity behavior, value objects, calculated properties, proficiency bonuses
- **Game Logic Services** (64 tests): Dice rolling, rules engine, combat mechanics
  - DiceRollerService: Cryptographic RNG, advantage/disadvantage, formula parsing
  - RulesEngineService: Ability checks, saving throws, attacks, damage calculation
  - CombatService: Initiative, attack resolution, damage/healing application
- **Application Services** (14 tests): CRUD operations, business logic, error handling
- **Validation** (37 tests): Request validation with FluentValidation
- **Integration** (planned): Database operations, SignalR hubs

### Test Examples

```csharp
[Theory]
[InlineData(1, 2)]   // Level 1 -> Proficiency Bonus +2
[InlineData(5, 3)]   // Level 5 -> Proficiency Bonus +3
[InlineData(20, 6)]  // Level 20 -> Proficiency Bonus +6
public void ProficiencyBonus_CalculatesCorrectlyByLevel(int level, int expectedBonus)
{
    var character = CreateTestCharacter();
    character.Level = level;
    
    character.ProficiencyBonus.Should().Be(expectedBonus);
}
```

## 📈 Development Roadmap

This is a 16-week implementation plan. See [docs/roadmap.md](docs/roadmap.md) for detailed tasks, acceptance criteria, and test coverage goals.

### Phase 1: Foundation ✅ (Weeks 1-2) - COMPLETED
- [x] .NET solution structure with Clean Architecture
- [x] Entity Framework Core with SQLite
- [x] Core domain models (Character, Session, Player, Message, DiceRoll)
- [x] REST API endpoints with FluentValidation
- [x] **48 unit tests passing** with 80%+ coverage

### Phase 2: Game Logic ✅ (Weeks 3-4) - COMPLETED
- [x] Cryptographically secure dice rolling system with `RandomNumberGenerator`
- [x] D&D 5e rules engine (ability checks, saving throws, attacks)
- [x] Combat mechanics (initiative, attacks, damage, healing)
- [x] Advantage/disadvantage mechanics for rolls
- [x] Critical hits and fumbles handling
- [x] Conditions system entities (15 D&D 5e conditions)
- [x] Entity Framework configurations for all game entities
- [x] Database migration applied successfully (SQLite)
- [x] **130 unit tests passing** (exceeded 60+ target)

### Phase 3: LLM Integration ✅ (Weeks 5-6) - COMPLETED
- [x] OpenAI SDK 2.6.0 integration with ChatClient
- [x] System prompt templates for DM (5 scenario types)
- [x] Conversation context management with session history
- [x] Streaming response handling via Server-Sent Events
- [x] Content moderation with keyword filtering
- [x] Retry policies with Polly (3 retries, exponential backoff)
- [x] Token usage tracking and cost estimation
- [x] **174 tests passing** (164 unit + 10 integration)

### Phase 4: Real-Time Features (Weeks 7-8)
- [ ] SignalR hubs for multiplayer sessions
- [ ] WebSocket client in Blazor
- [ ] Turn-based combat synchronization
- [ ] Presence tracking and reconnection logic
- [ ] Real-time dice rolling and messaging
- [ ] **Target: 50+ integration tests**

### Phase 5: Blazor Web UI (Weeks 9-10)
- [ ] Responsive layout with Tailwind CSS
- [ ] Character sheet and creation wizard
- [ ] Chat panel with DM/player differentiation
- [ ] Dice roller and initiative tracker
- [ ] Session lobby and management
- [ ] **Target: 40+ component tests (bUnit)**

### Phase 6: MAUI Mobile App (Weeks 11-12)
- [ ] .NET MAUI Blazor Hybrid setup
- [ ] MVVM pattern with CommunityToolkit
- [ ] Offline data sync with SQLite
- [ ] Push notifications and biometric auth
- [ ] Platform-specific services (iOS/Android)
- [ ] **Target: 35+ unit tests**

### Phase 7: Testing & Polish (Weeks 13-14)
- [ ] Comprehensive test suite (500+ tests)
- [ ] Performance optimization and caching
- [ ] Security audit (OWASP Top 10)
- [ ] Accessibility audit (WCAG 2.1 AA)
- [ ] Load testing SignalR hubs
- [ ] **Target: 80%+ code coverage**

### Phase 8: Deployment (Weeks 15-16)
- [ ] Docker images for API and Blazor Server
- [ ] Docker Compose with PostgreSQL and Redis
- [ ] Nginx reverse proxy with SSL
- [ ] CI/CD pipelines (GitHub Actions)
- [ ] Prometheus + Grafana monitoring
- [ ] App store submissions (iOS/Android)

**Current Progress**: Phase 3 Complete | **Next**: Phase 4 - Real-Time Features (SignalR)

See [docs/roadmap.md](docs/roadmap.md) and [docs/llm-integration-guide.md](docs/llm-integration-guide.md) for detailed implementation and configuration.

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Follow** coding standards (see [docs/.github/copilot-instructions.md](.github/copilot-instructions.md))
4. **Write** tests for new features
5. **Ensure** all tests pass (`dotnet test`)
6. **Commit** with clear messages (`git commit -m 'Add amazing feature'`)
7. **Push** to your branch (`git push origin feature/amazing-feature`)
8. **Open** a Pull Request

### Coding Standards

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use file-scoped namespaces
- Prefer `async/await` for I/O operations
- Write XML documentation for public APIs
- Maintain test coverage above 80%

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 👥 Authors

- **Shawn Chesney** - *Initial work* - [@SDCHESNEY](https://github.com/SDCHESNEY)

## 🙏 Acknowledgments

- **Wizards of the Coast** - D&D 5e System Reference Document
- **OpenAI/Anthropic** - LLM APIs
- **.NET Foundation** - Amazing framework and tools
- **Community** - All contributors and testers

## 📞 Support

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/SDCHESNEY/DNDGame-DotNet/issues)
- **Discussions**: [GitHub Discussions](https://github.com/SDCHESNEY/DNDGame-DotNet/discussions)

## 🔗 Related Projects

- [D&D 5e API](https://www.dnd5eapi.co/)
- [OpenAI .NET SDK](https://github.com/openai/openai-dotnet)
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)

---

**Built with ❤️ using .NET 9.0**
