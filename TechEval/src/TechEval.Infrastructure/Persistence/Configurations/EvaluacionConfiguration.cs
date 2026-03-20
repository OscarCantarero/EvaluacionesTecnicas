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

        builder.Property(e => e.ModoSeleccionPreguntas)
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(e => e.CantidadPreguntasSesion);
        // DistribucionDificultad is an owned type stored as columns on the same table
        builder.OwnsOne(e => e.DistribucionDificultad, dist =>
        {
            dist.Property(d => d.Facil).HasColumnName("distribucion_facil");
            dist.Property(d => d.Medio).HasColumnName("distribucion_medio");
            dist.Property(d => d.Dificil).HasColumnName("distribucion_dificil");
        });
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
        builder.Property(p => p.CategoriaId).HasColumnName("categoria_id");
        builder.HasIndex(p => p.CategoriaId);

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

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Descripcion).HasMaxLength(1000);
        builder.Property(c => c.CreadoPor).IsRequired().HasMaxLength(450);
        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
