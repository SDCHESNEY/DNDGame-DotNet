using DNDGame.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNDGame.Infrastructure.Data.Configurations;

public class ConditionConfiguration : IEntityTypeConfiguration<Condition>
{
    public void Configure(EntityTypeBuilder<Condition> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CharacterId)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(c => c.Duration)
            .IsRequired(false);

        builder.Property(c => c.SaveDC)
            .IsRequired(false);

        builder.Property(c => c.AppliedAt)
            .IsRequired();

        builder.Property(c => c.Source)
            .HasMaxLength(200)
            .IsRequired(false);

        // Relationships
        builder.HasOne(c => c.Character)
            .WithMany()
            .HasForeignKey(c => c.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.CharacterId)
            .HasDatabaseName("IX_Conditions_CharacterId");

        builder.HasIndex(c => c.Type)
            .HasDatabaseName("IX_Conditions_Type");

        builder.HasIndex(c => c.AppliedAt)
            .HasDatabaseName("IX_Conditions_AppliedAt");
    }
}
