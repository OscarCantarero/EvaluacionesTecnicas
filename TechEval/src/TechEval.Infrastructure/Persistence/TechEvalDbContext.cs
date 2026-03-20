using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Resultados;
using TechEval.Domain.Sesiones;
using TechEval.Infrastructure.Identity;

namespace TechEval.Infrastructure.Persistence;

public sealed class TechEvalDbContext(DbContextOptions<TechEvalDbContext> options)
    : IdentityDbContext<UsuarioApp, IdentityRole, string>(options)
{
    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();
    public DbSet<Pregunta> Preguntas => Set<Pregunta>();
    public DbSet<OpcionRespuesta> OpcionesRespuesta => Set<OpcionRespuesta>();
    public DbSet<RefreshTokenEntry> RefreshTokens => Set<RefreshTokenEntry>();
    public DbSet<SesionEvaluacion> Sesiones => Set<SesionEvaluacion>();
    public DbSet<PreguntaSesion> PreguntasSesion => Set<PreguntaSesion>();
    public DbSet<RespuestaCandidato> RespuestasCandidato => Set<RespuestaCandidato>();
    public DbSet<ResultadoEvaluacion> Resultados => Set<ResultadoEvaluacion>();
    public DbSet<PuntuacionPregunta> Puntuaciones => Set<PuntuacionPregunta>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(TechEvalDbContext).Assembly);
    }
}
