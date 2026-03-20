using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechEval.Domain.Sesiones;

namespace TechEval.Infrastructure.Persistence.Configurations;

public sealed class SesionEvaluacionConfiguration : IEntityTypeConfiguration<SesionEvaluacion>
{
    public void Configure(EntityTypeBuilder<SesionEvaluacion> builder)
    {
        builder.ToTable("sesiones_evaluacion");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.EvaluacionId)
            .IsRequired();

        builder.Property(s => s.CandidatoId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.CodigoAcceso)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Estado)
            .IsRequired();

        builder.Property(s => s.CreadaEn)
            .IsRequired();

        builder.Property(s => s.IniciadaEn);

        builder.Property(s => s.CompletadaEn);

        builder.Property(s => s.ContadorViolacionesPestana)
            .HasDefaultValue(0);

        // Índices
        builder.HasIndex(s => s.CodigoAcceso).IsUnique();
        builder.HasIndex(s => s.CandidatoId);
        builder.HasIndex(s => s.EvaluacionId);

        // Relación con preguntas (backing field privado)
        builder.HasMany(s => s.Preguntas)
            .WithOne()
            .HasForeignKey("SesionEvaluacionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Preguntas).HasField("_preguntas");
    }
}

public sealed class PreguntaSesionConfiguration : IEntityTypeConfiguration<PreguntaSesion>
{
    public void Configure(EntityTypeBuilder<PreguntaSesion> builder)
    {
        builder.ToTable("preguntas_sesion");

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.Id)
            .ValueGeneratedNever();

        builder.Property(ps => ps.EvaluacionId)
            .IsRequired();

        builder.Property(ps => ps.PreguntaId)
            .IsRequired();

        builder.Property(ps => ps.Texto)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ps => ps.Orden)
            .IsRequired();

        builder.Property(ps => ps.LimiteTiempoSegundos);

        builder.Property(ps => ps.FueRespondida)
            .HasDefaultValue(false);

        builder.Property(ps => ps.TiempoExpirado)
            .HasDefaultValue(false);

        // Índices
        builder.HasIndex(ps => ps.PreguntaId);
        builder.HasIndex("SesionEvaluacionId");
    }
}

public sealed class RespuestaCandidatoConfiguration : IEntityTypeConfiguration<RespuestaCandidato>
{
    public void Configure(EntityTypeBuilder<RespuestaCandidato> builder)
    {
        builder.ToTable("respuestas_candidato");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.PreguntaId)
            .IsRequired();

        builder.Property(r => r.Texto)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(r => r.UrlAdjunto)
            .HasMaxLength(500);

        builder.Property(r => r.TiempoEmpleadoSegundos)
            .IsRequired();

        builder.Property(r => r.BrindadaEn)
            .IsRequired();

        builder.Property(r => r.FueExpiratoElTiempo)
            .HasDefaultValue(false);

        // Índices
        builder.HasIndex(r => r.PreguntaId);
    }
}
