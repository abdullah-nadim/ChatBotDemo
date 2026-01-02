using ChatBotDemo.Models;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace ChatBotDemo.Data;

public class ChatBotDbContext : DbContext
{
    public ChatBotDbContext(DbContextOptions<ChatBotDbContext> options)
        : base(options)
    {
    }

    public DbSet<Context> Contexts { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Context>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.Title);

            // Configure vector embedding column
            // 1536 dimensions for OpenAI text-embedding-3-small
            // 768 dimensions for Gemini text-embedding-004
            // Using 1536 to support both (smaller embeddings will be padded if needed)
            entity.Property(e => e.Embedding)
                  .HasColumnType("vector(1536)");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Question).IsRequired();
            entity.Property(e => e.Answer).IsRequired();
            entity.HasOne(e => e.Context)
                  .WithMany()
                  .HasForeignKey(e => e.ContextId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

