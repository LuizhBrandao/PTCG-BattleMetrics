using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PTCGBattleMetrics.Domain.Entities;

namespace PTCGBattleMetrics.Infrastructure.Persistence;

public class BattleMetricsDbContext : DbContext
{
    public BattleMetricsDbContext(DbContextOptions<BattleMetricsDbContext> options) : base(options)
    {
    }

    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<GameDetail> GameDetails => Set<GameDetail>();
    public DbSet<MetaArchetype> MetaArchetypes => Set<MetaArchetype>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var stringListComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList()
        );

        // Deck configuration
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(150);
            entity.Property(d => d.Archetype).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Version).HasMaxLength(50);
            entity.Property(d => d.TechCards)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(stringListComparer);

            entity.HasMany(d => d.Cards)
                .WithOne(c => c.Deck)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Matches)
                .WithOne(m => m.Deck)
                .HasForeignKey(m => m.DeckId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DeckCard configuration
        modelBuilder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.Property(c => c.SetCode).HasMaxLength(10);
            entity.Property(c => c.CollectorNumber).HasMaxLength(20);
        });

        // Tournament configuration
        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.StoreOrVenue).HasMaxLength(200);

            entity.HasMany(t => t.Matches)
                .WithOne(m => m.Tournament)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Match configuration
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.OpponentArchetype).IsRequired().HasMaxLength(100);
            entity.Property(m => m.OpponentName).HasMaxLength(100);
            entity.Property(m => m.OpponentPopId).HasMaxLength(50);
            entity.Property(m => m.StartingActivePokemon).HasMaxLength(100);
            entity.Property(m => m.TacticalNotes).HasMaxLength(1000);

            entity.Property(m => m.TechCardsUsed)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .Metadata.SetValueComparer(stringListComparer);

            entity.HasMany(m => m.Games)
                .WithOne(g => g.Match)
                .HasForeignKey(g => g.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => m.OpponentArchetype);
            entity.HasIndex(m => m.CreatedAt);
            entity.HasIndex(m => m.Result);
        });

        // GameDetail configuration
        modelBuilder.Entity<GameDetail>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.StartingActivePokemon).HasMaxLength(100);
            entity.Property(g => g.Notes).HasMaxLength(500);
        });

        // MetaArchetype configuration
        modelBuilder.Entity<MetaArchetype>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PrimaryType).HasMaxLength(50);
            entity.Property(a => a.ColorHex).HasMaxLength(20);
            entity.HasIndex(a => a.Name).IsUnique();
        });
    }
}
