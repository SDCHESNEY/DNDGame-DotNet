# DNDGame - LLM-Powered Dungeon Master

A Dungeons & Dragons-style RPG powered by Large Language Models, built with .NET 9.0, Blazor, and .NET MAUI.

[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)
[![Tests](https://img.shields.io/badge/tests-48%20passing-success)](tests/)

## 🎮 Overview

DNDGame is an innovative RPG that combines classic Dungeons & Dragons gameplay with modern AI technology. An LLM acts as your Dungeon Master, creating dynamic narratives, managing game state, and responding to player actions in real-time.

### Key Features

- **AI Dungeon Master**: Leverages LLMs (OpenAI, Anthropic, or local models) to generate immersive storylines
- **D&D 5e Rules**: Authentic ability scores, proficiency bonuses, and character progression
- **Cross-Platform**: 
  - Web application (Blazor Server/WebAssembly)
  - Mobile apps (iOS/Android via .NET MAUI)
  - REST API for third-party integrations
- **Real-Time Gameplay**: SignalR-powered WebSocket communication
- **Persistent World**: SQL Server database with Entity Framework Core
- **Character Management**: Full CRUD operations for D&D characters
- **Session System**: Solo and multiplayer campaign support

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
| **Database** | SQL Server / LocalDB |
| **Validation** | FluentValidation 12.1 |
| **Web UI** | Blazor Server/WebAssembly (Phase 5) |
| **Mobile** | .NET MAUI (Phase 6) |
| **Real-Time** | SignalR (Phase 3) |
| **Testing** | xUnit, Moq, FluentAssertions |
| **AI Integration** | OpenAI/Anthropic SDKs (Phase 2) |

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/) or SQL Server LocalDB
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
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

3. **Update database connection string**
   
   Edit `src/DNDGame.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DndGame;Trusted_Connection=true;TrustServerCertificate=true"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project src/DNDGame.API
   ```
   
   Or if using dotnet-ef tools:
   ```bash
   export PATH="$PATH:$HOME/.dotnet/tools"
   dotnet ef database update --project src/DNDGame.API
   ```

5. **Run the API**
   ```bash
   cd src/DNDGame.API
   dotnet run
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

Current test status: **48/48 passing** ✅

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

- **Domain Logic** (9 tests): Entity behavior, value objects, calculated properties
- **Service Layer** (14 tests): CRUD operations, business logic, error handling
- **Validation** (11 tests): Request validation with FluentValidation
- **Repositories** (planned): Database operations with in-memory provider

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
- [x] Entity Framework Core with SQL Server
- [x] Core domain models (Character, Session, Player, Message, DiceRoll)
- [x] REST API endpoints with FluentValidation
- [x] **48 unit tests passing** with 80%+ coverage

### Phase 2: Game Logic (Weeks 3-4)
- [ ] Cryptographically secure dice rolling system
- [ ] D&D 5e rules engine (ability checks, saving throws)
- [ ] Combat mechanics (initiative, attacks, damage)
- [ ] Character progression and level-up logic
- [ ] Conditions system (stunned, prone, etc.)
- [ ] **Target: 60+ unit tests**

### Phase 3: LLM Integration (Weeks 5-6)
- [ ] OpenAI/Anthropic SDK integration
- [ ] System prompt templates for DM
- [ ] Conversation context management
- [ ] Streaming response handling
- [ ] Content moderation and rate limiting
- [ ] **Target: 40+ unit tests, 10+ integration tests**

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

**Current Progress**: Phase 1 Complete | **Next**: Phase 2 - Game Logic

See [docs/roadmap.md](docs/roadmap.md) for detailed implementation plan.

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
