using DNDGame.Application.Services;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class CombatServiceTests
{
    private readonly Mock<ICharacterRepository> _mockCharacterRepo;
    private readonly Mock<ISessionRepository> _mockSessionRepo;
    private readonly Mock<IDiceRoller> _mockDiceRoller;
    private readonly Mock<IRulesEngine> _mockRulesEngine;
    private readonly Mock<ILogger<CombatService>> _mockLogger;
    private readonly CombatService _sut;

    public CombatServiceTests()
    {
        _mockCharacterRepo = new Mock<ICharacterRepository>();
        _mockSessionRepo = new Mock<ISessionRepository>();
        _mockDiceRoller = new Mock<IDiceRoller>();
        _mockRulesEngine = new Mock<IRulesEngine>();
        _mockLogger = new Mock<ILogger<CombatService>>();
        
        _sut = new CombatService(
            _mockCharacterRepo.Object,
            _mockSessionRepo.Object,
            _mockDiceRoller.Object,
            _mockRulesEngine.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RollInitiativeAsync_WithValidSession_ReturnsInitiativeOrderDescending()
    {
        // Arrange
        var session = new Session
        {
            Id = 1,
            Title = "Test Session",
            Mode = SessionMode.Multiplayer,
            State = SessionState.InProgress,
            Participants = new List<SessionParticipant>
            {
                new() { SessionId = 1, CharacterId = 1, IsActive = true },
                new() { SessionId = 1, CharacterId = 2, IsActive = true },
                new() { SessionId = 1, CharacterId = 3, IsActive = true }
            }
        };

        var characters = new List<Character>
        {
            new()
            {
                Id = 1,
                Name = "Fighter",
                AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
                HitPoints = 25,
                MaxHitPoints = 25
            },
            new()
            {
                Id = 2,
                Name = "Rogue",
                AbilityScores = new AbilityScores(10, 18, 12, 14, 13, 10),
                HitPoints = 20,
                MaxHitPoints = 20
            },
            new()
            {
                Id = 3,
                Name = "Wizard",
                AbilityScores = new AbilityScores(8, 12, 13, 18, 14, 10),
                HitPoints = 15,
                MaxHitPoints = 15
            }
        };

        _mockSessionRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(characters[0]);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(characters[1]);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(characters[2]);

        // Mock dice rolls: Fighter=15, Rogue=18, Wizard=12
        _mockDiceRoller.SetupSequence(d => d.Roll("1d20"))
            .Returns(new Core.Models.DiceRollResult { Formula = "1d20", Total = 15, IndividualRolls = [15], Modifier = 0, Timestamp = DateTime.UtcNow })
            .Returns(new Core.Models.DiceRollResult { Formula = "1d20", Total = 18, IndividualRolls = [18], Modifier = 0, Timestamp = DateTime.UtcNow })
            .Returns(new Core.Models.DiceRollResult { Formula = "1d20", Total = 12, IndividualRolls = [12], Modifier = 0, Timestamp = DateTime.UtcNow });

        // Act
        var result = await _sut.RollInitiativeAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result[0].CharacterName.Should().Be("Rogue"); // 18 + 4 DEX = 22
        result[1].CharacterName.Should().Be("Fighter"); // 15 + 2 DEX = 17
        result[2].CharacterName.Should().Be("Wizard"); // 12 + 1 DEX = 13
        
        result[0].InitiativeRoll.Should().Be(22);
        result[1].InitiativeRoll.Should().Be(17);
        result[2].InitiativeRoll.Should().Be(13);
    }

    [Fact]
    public async Task RollInitiativeAsync_WithInvalidSession_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockSessionRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync((Session?)null);

        // Act
        var act = async () => await _sut.RollInitiativeAsync(999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Session*not found*");
    }

    [Fact]
    public async Task ResolveAttackAsync_WithSuccessfulHit_ReturnsDamageResult()
    {
        // Arrange
        var attacker = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            Level = 5
        };

        var defender = new Character
        {
            Id = 2,
            Name = "Goblin",
            AbilityScores = new AbilityScores(8, 14, 10, 10, 8, 8),
            ArmorClass = 13
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(attacker);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(defender);

        _mockRulesEngine.Setup(r => r.ResolveAttack(6, 13, AdvantageType.Normal))
            .Returns(new Core.Models.AttackResult
            {
                AttackRoll = 18,
                TargetAC = 13,
                Hit = true,
                IsCritical = false,
                IsFumble = false
            });

        _mockRulesEngine.Setup(r => r.CalculateDamage("1d8+3", false))
            .Returns(9);

        // Act
        var result = await _sut.ResolveAttackAsync(1, 2, "1d8+3");

        // Assert
        result.Should().NotBeNull();
        result.Hit.Should().BeTrue();
        result.Damage.Should().Be(9);
        result.AttackRoll.Should().Be(18);
    }

    [Fact]
    public async Task ResolveAttackAsync_WithCriticalHit_DoublesDamage()
    {
        // Arrange
        var attacker = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            Level = 5
        };

        var defender = new Character
        {
            Id = 2,
            Name = "Goblin",
            AbilityScores = new AbilityScores(8, 14, 10, 10, 8, 8),
            ArmorClass = 13
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(attacker);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(defender);

        _mockRulesEngine.Setup(r => r.ResolveAttack(6, 13, AdvantageType.Normal))
            .Returns(new Core.Models.AttackResult
            {
                AttackRoll = 23,
                TargetAC = 13,
                Hit = true,
                IsCritical = true,
                IsFumble = false
            });

        _mockRulesEngine.Setup(r => r.CalculateDamage("1d8+3", true))
            .Returns(15); // Critical damage

        // Act
        var result = await _sut.ResolveAttackAsync(1, 2, "1d8+3");

        // Assert
        result.IsCritical.Should().BeTrue();
        result.Damage.Should().Be(15);
        _mockRulesEngine.Verify(r => r.CalculateDamage("1d8+3", true), Times.Once);
    }

    [Fact]
    public async Task ResolveAttackAsync_WithMiss_ReturnsZeroDamage()
    {
        // Arrange
        var attacker = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            Level = 5
        };

        var defender = new Character
        {
            Id = 2,
            Name = "Goblin",
            AbilityScores = new AbilityScores(8, 14, 10, 10, 8, 8),
            ArmorClass = 18
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(attacker);
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(defender);

        _mockRulesEngine.Setup(r => r.ResolveAttack(6, 18, AdvantageType.Normal))
            .Returns(new Core.Models.AttackResult
            {
                AttackRoll = 12,
                TargetAC = 18,
                Hit = false,
                IsCritical = false,
                IsFumble = false
            });

        // Act
        var result = await _sut.ResolveAttackAsync(1, 2, "1d8+3");

        // Assert
        result.Hit.Should().BeFalse();
        result.Damage.Should().Be(0);
        _mockRulesEngine.Verify(r => r.CalculateDamage(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ResolveAttackAsync_WithInvalidAttacker_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var act = async () => await _sut.ResolveAttackAsync(999, 1, "1d8+3");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Attacker*not found*");
    }

    [Fact]
    public async Task ApplyDamageAsync_WithNonLethalDamage_ReducesHPAndReturnsTrue()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 25,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var isConscious = await _sut.ApplyDamageAsync(1, 10);

        // Assert
        isConscious.Should().BeTrue();
        character.HitPoints.Should().Be(15); // 25 - 10
        _mockCharacterRepo.Verify(r => r.UpdateAsync(character, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyDamageAsync_WithLethalDamage_ReducesHPToZeroAndReturnsFalse()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 10,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var isConscious = await _sut.ApplyDamageAsync(1, 15);

        // Assert
        isConscious.Should().BeFalse();
        character.HitPoints.Should().Be(0); // Cannot go below 0
        _mockCharacterRepo.Verify(r => r.UpdateAsync(character, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyDamageAsync_WithExcessDamage_CapsAtZero()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 5,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var isConscious = await _sut.ApplyDamageAsync(1, 50);

        // Assert
        character.HitPoints.Should().Be(0);
        character.HitPoints.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ApplyHealingAsync_WithNormalHealing_IncreasesHP()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 10,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var newHP = await _sut.ApplyHealingAsync(1, 8);

        // Assert
        newHP.Should().Be(18); // 10 + 8
        character.HitPoints.Should().Be(18);
        _mockCharacterRepo.Verify(r => r.UpdateAsync(character, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplyHealingAsync_WithExcessHealing_CapsAtMaxHP()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 25,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var newHP = await _sut.ApplyHealingAsync(1, 20);

        // Assert
        newHP.Should().Be(30); // Capped at MaxHitPoints
        character.HitPoints.Should().Be(30);
        character.HitPoints.Should().BeLessThanOrEqualTo(character.MaxHitPoints);
    }

    [Fact]
    public async Task ApplyHealingAsync_AtMaxHP_RemainsAtMaxHP()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 30,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var newHP = await _sut.ApplyHealingAsync(1, 10);

        // Assert
        newHP.Should().Be(30);
        character.HitPoints.Should().Be(30);
    }

    [Fact]
    public async Task ApplyHealingAsync_WithInvalidCharacter_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var act = async () => await _sut.ApplyHealingAsync(999, 10);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Character*not found*");
    }

    [Fact]
    public async Task ApplyDamageAsync_WithZeroDamage_DoesNotChangeHP()
    {
        // Arrange
        var character = new Character
        {
            Id = 1,
            Name = "Fighter",
            AbilityScores = new AbilityScores(16, 14, 15, 10, 12, 8),
            HitPoints = 20,
            MaxHitPoints = 30
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var isConscious = await _sut.ApplyDamageAsync(1, 0);

        // Assert
        isConscious.Should().BeTrue();
        character.HitPoints.Should().Be(20);
    }
}
