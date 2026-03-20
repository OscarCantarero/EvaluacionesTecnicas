using MediatR;
using TechEval.Domain.Evaluaciones.Repositorios;

namespace TechEval.Application.Categorias.Queries.ListarCategorias;

public sealed record CategoriaDto(Guid Id, string Nombre, string? Descripcion, DateTime CreadoEn);

public sealed record ListarCategoriasQuery : IRequest<List<CategoriaDto>>;

public sealed class ListarCategoriasQueryHandler(IRepositorioCategoria repositorio)
    : IRequestHandler<ListarCategoriasQuery, List<CategoriaDto>>
{
    public async Task<List<CategoriaDto>> Handle(ListarCategoriasQuery query, CancellationToken cancellationToken)
    {
        var categorias = await repositorio.ListarAsync(cancellationToken);
        return categorias
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Descripcion, c.CreadoEn))
            .ToList();
    }
}
