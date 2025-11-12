using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDGame.Core.Entities;

namespace DNDGame.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Mode)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.State)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.CurrentScene)
            .HasMaxLength(2000);

        builder.Property(s => s.WorldFlags)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.LastActivityAt);

        builder.HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.DiceRolls)
            .WithOne(d => d.Session)
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.State);
        builder.HasIndex(s => s.CreatedAt);
    }
}
