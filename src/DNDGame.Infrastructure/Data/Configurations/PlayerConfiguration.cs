using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDGame.Core.Entities;

namespace DNDGame.Infrastructure.Data.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => p.UserId);

        builder.HasMany(p => p.Characters)
            .WithOne(c => c.Player)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
