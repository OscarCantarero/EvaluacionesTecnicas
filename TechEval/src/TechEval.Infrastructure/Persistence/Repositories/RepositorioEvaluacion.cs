using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Infrastructure.Persistence.Repositories;

public sealed class RepositorioEvaluacion(TechEvalDbContext context) : IRepositorioEvaluacion
{
    public async Task<Evaluacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Evaluaciones.FindAsync([id], cancellationToken);

    public async Task<Evaluacion?> ObtenerConPreguntasAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Evaluaciones
            .Include(e => e.Preguntas)
            .ThenInclude(p => p.Opciones)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Evaluacion>> ListarAsync(string? evaluadorId, CancellationToken cancellationToken = default)
    {
        var query = context.Evaluaciones
            .Include(e => e.Preguntas)
            .AsQueryable();

        if (!string.IsNullOrEmpty(evaluadorId))
            query = query.Where(e => e.CreadoPor == evaluadorId);

        return await query.OrderByDescending(e => e.CreadoEn).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken = default) =>
        await context.Evaluaciones.AnyAsync(
            e => e.Nombre == nombre && (excluirId == null || e.Id != excluirId),
            cancellationToken);

    public async Task AgregarAsync(Evaluacion agregado, CancellationToken cancellationToken = default)
    {
        await context.Evaluaciones.AddAsync(agregado, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Evaluacion agregado, CancellationToken cancellationToken = default)
    {
        if (context.Entry(agregado).State == EntityState.Detached)
            context.Evaluaciones.Attach(agregado);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(Evaluacion agregado, CancellationToken cancellationToken = default)
    {
        context.Evaluaciones.Remove(agregado);
        await context.SaveChangesAsync(cancellationToken);
    }
}
