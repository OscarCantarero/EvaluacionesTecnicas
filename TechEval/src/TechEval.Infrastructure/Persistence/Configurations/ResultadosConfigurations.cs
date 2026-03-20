using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechEval.Domain.Resultados;

namespace TechEval.Infrastructure.Persistence.Configurations;

public sealed class ResultadoEvaluacionConfiguration : IEntityTypeConfiguration<ResultadoEvaluacion>
{
    public void Configure(EntityTypeBuilder<ResultadoEvaluacion> builder)
    {
        builder.ToTable("resultados_evaluacion");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.SesionId)
            .HasColumnName("sesion_id")
            .IsRequired();

        builder.Property(r => r.EvaluacionId)
            .HasColumnName("evaluacion_id")
            .IsRequired();

        builder.Property(r => r.CandidatoId)
            .HasColumnName("candidato_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(r => r.NombreCandidato)
            .HasColumnName("nombre_candidato")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.TituloEvaluacion)
            .HasColumnName("titulo_evaluacion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.FechaCompletacion)
            .HasColumnName("fecha_completacion")
            .IsRequired();

        builder.Property(r => r.PuntuacionTotal)
            .HasColumnName("puntuacion_total")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.PuntuacionMaxima)
            .HasColumnName("puntuacion_maxima")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.PorcentajeObtenido)
            .HasColumnName("porcentaje_obtenido")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.EstadoRevision)
            .HasColumnName("estado_revision")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.FechaInicioRevision)
            .HasColumnName("fecha_inicio_revision");

        builder.Property(r => r.FechaFinRevision)
            .HasColumnName("fecha_fin_revision");

        builder.Property(r => r.EvaluadorRevision)
            .HasColumnName("evaluador_revision")
            .HasMaxLength(256);

        builder.Property(r => r.ViolacionesPestana)
            .HasColumnName("violaciones_pestana")
            .IsRequired();

        builder.Property(r => r.TiempoTotalSegundos)
            .HasColumnName("tiempo_total_segundos")
            .IsRequired();

        // Relación con PuntuacionPregunta
        builder.HasMany(r => r.Puntuaciones)
            .WithOne()
            .HasForeignKey("ResultadoEvaluacionId")
            .IsRequired();

        builder.Navigation(r => r.Puntuaciones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Relación con TranscripcionEvaluacion
        builder.HasMany(r => r.Transcripciones)
            .WithOne()
            .HasForeignKey("ResultadoEvaluacionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Transcripciones)
            .HasField("_transcripciones")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índices
        builder.HasIndex(r => r.SesionId)
            .IsUnique();
        
        builder.HasIndex(r => r.CandidatoId);
        
        builder.HasIndex(r => r.EvaluacionId);
        
        builder.HasIndex(r => r.EstadoRevision);
    }
}

public sealed class PuntuacionPreguntaConfiguration : IEntityTypeConfiguration<PuntuacionPregunta>
{
    public void Configure(EntityTypeBuilder<PuntuacionPregunta> builder)
    {
        builder.ToTable("puntuaciones_pregunta");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.PreguntaSesionId)
            .HasColumnName("pregunta_sesion_id")
            .IsRequired();

        builder.Property(p => p.NumeroPregunta)
            .HasColumnName("numero_pregunta")
            .IsRequired();

        builder.Property(p => p.TipoPregunta)
            .HasColumnName("tipo_pregunta")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Respuesta)
            .HasColumnName("respuesta")
            .IsRequired();

        builder.Property(p => p.PuntuacionAutomatica)
            .HasColumnName("puntuacion_automatica")
            .HasPrecision(5, 2);

        builder.Property(p => p.PuntuacionManual)
            .HasColumnName("puntuacion_manual")
            .HasPrecision(5, 2);

        builder.Property(p => p.PuntuacionIASugerida)
            .HasColumnName("puntuacion_ia_sugerida")
            .HasPrecision(5, 2);

        builder.Property(p => p.JustificacionIA)
            .HasColumnName("justificacion_ia");

        builder.Property(p => p.Observaciones)
            .HasColumnName("observaciones");

        builder.Property(p => p.FueExpirada)
            .HasColumnName("fue_expirada")
            .IsRequired();

        builder.Property(p => p.UrlAdjunto)
            .HasColumnName("url_adjunto");

        builder.Property(p => p.TiempoEmpleadoSegundos)
            .HasColumnName("tiempo_empleado_segundos")
            .IsRequired();

        // Índices
        builder.HasIndex(p => p.PreguntaSesionId);
    }
}

public sealed class TranscripcionEvaluacionConfiguration : IEntityTypeConfiguration<TranscripcionEvaluacion>
{
    public void Configure(EntityTypeBuilder<TranscripcionEvaluacion> builder)
    {
        builder.ToTable("transcripciones_evaluacion");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Tipo).HasConversion<int>().IsRequired();
        builder.Property(t => t.Contenido).HasMaxLength(50000);
        builder.Property(t => t.UrlArchivo).HasMaxLength(500);
        builder.Property(t => t.PuntajeIA).HasPrecision(5, 2);
        builder.Property(t => t.JustificacionIA).HasMaxLength(5000);
        builder.Property(t => t.Estado).HasConversion<int>().IsRequired();
        builder.Property(t => t.CreadaEn).IsRequired();
        builder.HasIndex(t => t.Tipo);
    }
}
