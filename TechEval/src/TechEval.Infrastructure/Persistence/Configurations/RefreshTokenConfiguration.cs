using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechEval.Infrastructure.Identity;

namespace TechEval.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntry>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntry> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Token).IsRequired().HasMaxLength(500);
        builder.Property(r => r.UsuarioId).IsRequired().HasMaxLength(450);
        builder.HasIndex(r => r.Token).IsUnique();
    }
}
