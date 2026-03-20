using Microsoft.EntityFrameworkCore;
using TechEval.Domain.Evaluaciones;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Infrastructure.Persistence.Repositories;

public sealed class RepositorioCategoria(TechEvalDbContext context) : IRepositorioCategoria
{
    public async Task<Categoria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Categorias.FindAsync([id], cancellationToken);

    public async Task<List<Categoria>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Categorias.OrderBy(c => c.Nombre).ToListAsync(cancellationToken);

    public async Task<bool> ExistePorNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken = default) =>
        await context.Categorias.AnyAsync(
            c => c.Nombre == nombre && (excluirId == null || c.Id != excluirId), cancellationToken);

    public async Task AgregarAsync(Categoria agregado, CancellationToken cancellationToken = default)
    {
        await context.Categorias.AddAsync(agregado, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Categoria agregado, CancellationToken cancellationToken = default)
    {
        if (context.Entry(agregado).State == EntityState.Detached)
            context.Categorias.Attach(agregado);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task EliminarAsync(Categoria agregado, CancellationToken cancellationToken = default)
    {
        context.Categorias.Remove(agregado);
        await context.SaveChangesAsync(cancellationToken);
    }
}
