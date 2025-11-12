using DNDGame.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNDGame.Infrastructure.Data.Configurations;

public class SessionParticipantConfiguration : IEntityTypeConfiguration<SessionParticipant>
{
    public void Configure(EntityTypeBuilder<SessionParticipant> builder)
    {
        // Composite primary key
        builder.HasKey(sp => new { sp.SessionId, sp.CharacterId });

        builder.Property(sp => sp.SessionId)
            .IsRequired();

        builder.Property(sp => sp.CharacterId)
            .IsRequired();

        builder.Property(sp => sp.JoinedAt)
            .IsRequired();

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(sp => sp.Session)
            .WithMany(s => s.Participants)
            .HasForeignKey(sp => sp.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Character)
            .WithMany()
            .HasForeignKey(sp => sp.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sp => sp.SessionId)
            .HasDatabaseName("IX_SessionParticipants_SessionId");

        builder.HasIndex(sp => sp.CharacterId)
            .HasDatabaseName("IX_SessionParticipants_CharacterId");

        builder.HasIndex(sp => sp.IsActive)
            .HasDatabaseName("IX_SessionParticipants_IsActive");
    }
}
