using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Sesiones;
using TechEval.Domain.Sesiones.Repositorios;

namespace TechEval.Infrastructure.Persistence.Repositories;

public sealed class RepositorioSesionEvaluacion : IRepositorioSesionEvaluacion
{
    private readonly TechEvalDbContext _context;

    public RepositorioSesionEvaluacion(TechEvalDbContext context)
    {
        _context = context;
    }

    public async Task<SesionEvaluacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SesionEvaluacion?> ObtenerConPreguntasAsync(Guid sesionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .Include(s => s.Preguntas)
                .ThenInclude(p => p.Respuesta)
            .FirstOrDefaultAsync(s => s.Id == sesionId, cancellationToken);
    }

    public async Task<SesionEvaluacion?> ObtenerPorCodigoAccesoAsync(string codigoAcceso, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .Include(s => s.Preguntas)
                .ThenInclude(p => p.Respuesta)
            .FirstOrDefaultAsync(s => s.CodigoAcceso == codigoAcceso, cancellationToken);
    }

    public async Task<List<SesionEvaluacion>> ObtenerPorCandidatoAsync(string candidatoId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .Where(s => s.CandidatoId == candidatoId)
            .OrderByDescending(s => s.CreadaEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SesionEvaluacion>> ObtenerPorEvaluacionAsync(Guid evaluacionId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .Where(s => s.EvaluacionId == evaluacionId)
            .OrderByDescending(s => s.CreadaEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SesionEvaluacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<SesionEvaluacion>()
            .OrderByDescending(s => s.CreadaEn)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(SesionEvaluacion entity, CancellationToken cancellationToken = default)
    {
        _context.Set<SesionEvaluacion>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(SesionEvaluacion entity, CancellationToken cancellationToken = default)
    {
        _context.Set<SesionEvaluacion>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(SesionEvaluacion entity, CancellationToken cancellationToken = default)
    {
        _context.Set<SesionEvaluacion>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
