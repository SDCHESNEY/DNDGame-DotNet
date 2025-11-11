using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDGame.Core.Entities;

namespace DNDGame.Infrastructure.Data.Configurations;

public class DiceRollConfiguration : IEntityTypeConfiguration<DiceRoll>
{
    public void Configure(EntityTypeBuilder<DiceRoll> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.RollerId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(d => d.Formula)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.IndividualRolls)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(d => d.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.Timestamp)
            .IsRequired();

        builder.HasIndex(d => d.SessionId);
        builder.HasIndex(d => d.Timestamp);
    }
}
