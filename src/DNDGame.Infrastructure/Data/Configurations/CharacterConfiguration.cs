using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDGame.Core.Entities;
using DNDGame.Core.ValueObjects;
using System.Text.Json;

namespace DNDGame.Infrastructure.Data.Configurations;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Class)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Level)
            .IsRequired();

        builder.OwnsOne(c => c.AbilityScores, ab =>
        {
            ab.Property(a => a.Strength).IsRequired();
            ab.Property(a => a.Dexterity).IsRequired();
            ab.Property(a => a.Constitution).IsRequired();
            ab.Property(a => a.Intelligence).IsRequired();
            ab.Property(a => a.Wisdom).IsRequired();
            ab.Property(a => a.Charisma).IsRequired();
        });

        builder.Property(c => c.HitPoints)
            .IsRequired();

        builder.Property(c => c.MaxHitPoints)
            .IsRequired();

        builder.Property(c => c.ArmorClass)
            .IsRequired();

        builder.Property(c => c.Skills)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(c => c.Inventory)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(c => c.PersonalityTraits)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Ignore(c => c.ProficiencyBonus);

        builder.HasIndex(c => c.PlayerId);
        builder.HasIndex(c => c.Name);
    }
}
