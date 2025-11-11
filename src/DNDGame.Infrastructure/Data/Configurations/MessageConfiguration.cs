using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DNDGame.Core.Entities;

namespace DNDGame.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.AuthorId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.Timestamp)
            .IsRequired();

        builder.HasIndex(m => m.SessionId);
        builder.HasIndex(m => m.Timestamp);
    }
}
