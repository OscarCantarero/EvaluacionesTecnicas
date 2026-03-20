using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechEval.Domain.Common.ValueObjects;
using TechEval.Domain.Evaluaciones;

namespace TechEval.Infrastructure.Persistence.Configurations;

public sealed class EvaluacionConfiguration : IEntityTypeConfiguration<Evaluacion>
{
    public void Configure(EntityTypeBuilder<Evaluacion> builder)
    {
        builder.ToTable("evaluaciones");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Descripcion).HasMaxLength(1000);
        builder.Property(e => e.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreadoPor).IsRequired().HasMaxLength(450);

        builder.HasMany(e => e.Preguntas)
            .WithOne()
            .HasForeignKey(p => p.EvaluacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Preguntas)
            .HasField("_preguntas")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PreguntaConfiguration : IEntityTypeConfiguration<Pregunta>
{
    public void Configure(EntityTypeBuilder<Pregunta> builder)
    {
        builder.ToTable("preguntas");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Texto).IsRequired().HasMaxLength(2000);
        builder.Property(p => p.TipoPregunta).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.NivelDificultad).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(p => p.Opciones)
            .WithOne()
            .HasForeignKey(o => o.PreguntaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Opciones)
            .HasField("_opciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class OpcionRespuestaConfiguration : IEntityTypeConfiguration<OpcionRespuesta>
{
    public void Configure(EntityTypeBuilder<OpcionRespuesta> builder)
    {
        builder.ToTable("opciones_respuesta");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Texto).IsRequired().HasMaxLength(1000);
    }
}
