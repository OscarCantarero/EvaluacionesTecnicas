using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Resultados;
using TechEval.Domain.Resultados.Repositorios;

namespace TechEval.Infrastructure.Persistence.Repositories;

public sealed class RepositorioResultadoEvaluacion : IRepositorioResultadoEvaluacion
{
    private readonly TechEvalDbContext _context;

    public RepositorioResultadoEvaluacion(TechEvalDbContext context)
    {
        _context = context;
    }

    public async Task<ResultadoEvaluacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Resultados
            .Include(r => r.Puntuaciones)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AgregarAsync(ResultadoEvaluacion resultado, CancellationToken cancellationToken = default)
    {
        _context.Resultados.Add(resultado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(ResultadoEvaluacion resultado, CancellationToken cancellationToken = default)
    {
        _context.Resultados.Update(resultado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(ResultadoEvaluacion resultado, CancellationToken cancellationToken = default)
    {
        _context.Resultados.Remove(resultado);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ResultadoEvaluacion?> ObtenerPorSesionAsync(Guid sesionId, CancellationToken cancellationToken = default)
    {
        return await _context.Resultados
            .Include(r => r.Puntuaciones)
            .FirstOrDefaultAsync(r => r.SesionId == sesionId, cancellationToken);
    }

    public async Task<List<ResultadoEvaluacion>> ObtenerPorCandidatoAsync(string candidatoId, CancellationToken cancellationToken = default)
    {
        return await _context.Resultados
            .Include(r => r.Puntuaciones)
            .Where(r => r.CandidatoId == candidatoId)
            .OrderByDescending(r => r.FechaCompletacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ResultadoEvaluacion>> ObtenerPorEvaluacionAsync(Guid evaluacionId, CancellationToken cancellationToken = default)
    {
        return await _context.Resultados
            .Include(r => r.Puntuaciones)
            .Where(r => r.EvaluacionId == evaluacionId)
            .OrderByDescending(r => r.FechaCompletacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ResultadoEvaluacion>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resultados
            .Include(r => r.Puntuaciones)
            .OrderByDescending(r => r.FechaCompletacion)
            .ToListAsync(cancellationToken);
    }
}
