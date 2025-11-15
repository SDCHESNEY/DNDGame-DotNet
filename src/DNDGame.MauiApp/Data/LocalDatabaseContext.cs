using Microsoft.EntityFrameworkCore;
using DNDGame.Core.Entities;

namespace DNDGame.MauiApp.Data;

public class LocalDatabaseContext : DbContext
{
    public LocalDatabaseContext(DbContextOptions<LocalDatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DiceRoll> DiceRolls => Set<DiceRoll>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Character configuration
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Class).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            entity.OwnsOne(e => e.AbilityScores);
            entity.Ignore(e => e.Player);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.Mode).IsRequired();
            entity.HasMany(e => e.Messages).WithOne(m => m.Session).HasForeignKey(m => m.SessionId);
            entity.HasMany(e => e.DiceRolls).WithOne(d => d.Session).HasForeignKey(d => d.SessionId);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Ignore(e => e.Session);
        });

        // DiceRoll configuration
        modelBuilder.Entity<DiceRoll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Formula).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Total).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Ignore(e => e.Session);
        });
    }
}
