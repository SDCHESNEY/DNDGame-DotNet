using Microsoft.EntityFrameworkCore;
using DNDGame.Core.Entities;
using System.Reflection;

namespace DNDGame.Infrastructure.Data;

public class DndGameContext : DbContext
{
    public DndGameContext(DbContextOptions<DndGameContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<DiceRoll> DiceRolls => Set<DiceRoll>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);
    }
}
