using DNDGame.Application.DTOs;
using DNDGame.Application.Services;
using DNDGame.Core.Entities;
using DNDGame.Core.Enums;
using DNDGame.Core.Interfaces;
using DNDGame.Core.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace DNDGame.UnitTests.Services;

public class CharacterServiceTests
{
    private readonly Mock<ICharacterRepository> _mockRepository;
    private readonly CharacterService _sut;

    public CharacterServiceTests()
    {
        _mockRepository = new Mock<ICharacterRepository>();
        _sut = new CharacterService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetCharacterAsync_WithValidId_ReturnsCharacterDto()
    {
        // Arrange
        var character = CreateTestCharacter(1, "Aragorn");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);

        // Act
        var result = await _sut.GetCharacterAsync(1);

        // Assert
        result.Should().NotBeNull();
        var dto = result as CharacterDto;
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(1);
        dto.Name.Should().Be("Aragorn");
    }

    [Fact]
    public async Task GetCharacterAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _sut.GetCharacterAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCharacterAsync_WithValidRequest_CreatesCharacter()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Legolas", CharacterClass.Ranger, 1,
            new AbilityScores(10, 18, 12, 14, 16, 10), 10, 15);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Character>(), It.IsAny<CancellationToken>()))
            .Callback<Character, CancellationToken>((c, _) => c.Id = 1)
            .ReturnsAsync((Character c, CancellationToken _) => c);

        // Act
        var result = await _sut.CreateCharacterAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        var dto = result as CharacterDto;
        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Legolas");
        dto.Class.Should().Be(CharacterClass.Ranger);
        dto.HitPoints.Should().Be(10); // Should start at full health
        
        _mockRepository.Verify(r => r.AddAsync(It.Is<Character>(c => 
            c.Name == "Legolas" && 
            c.PlayerId == 1 && 
            c.HitPoints == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCharacterAsync_WithValidId_UpdatesCharacter()
    {
        // Arrange
        var existingCharacter = CreateTestCharacter(1, "Gimli");
        var request = new CreateCharacterRequest(
            1, "Gimli the Brave", CharacterClass.Fighter, 5,
            new AbilityScores(18, 10, 16, 8, 12, 10), 50, 18,
            null, null, "Brave and loyal");

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingCharacter);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Character>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateCharacterAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        var dto = result as CharacterDto;
        dto.Should().NotBeNull();
        dto!.Name.Should().Be("Gimli the Brave");
        dto.Level.Should().Be(5);
        
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Character>(c => 
            c.Name == "Gimli the Brave" && c.Level == 5), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCharacterAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var request = new CreateCharacterRequest(
            1, "Test", CharacterClass.Fighter, 1,
            new AbilityScores(10, 10, 10, 10, 10, 10), 10, 10);

        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _sut.UpdateCharacterAsync(999, request);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Character>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCharacterAsync_WithValidId_DeletesCharacter()
    {
        // Arrange
        var character = CreateTestCharacter(1, "Frodo");
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(character);
        _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteCharacterAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCharacterAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _sut.DeleteCharacterAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Character CreateTestCharacter(int id, string name)
    {
        return new Character
        {
            Id = id,
            PlayerId = 1,
            Name = name,
            Class = CharacterClass.Fighter,
            Level = 1,
            AbilityScores = new AbilityScores(10, 10, 10, 10, 10, 10),
            MaxHitPoints = 10,
            HitPoints = 10,
            ArmorClass = 10
        };
    }
}
