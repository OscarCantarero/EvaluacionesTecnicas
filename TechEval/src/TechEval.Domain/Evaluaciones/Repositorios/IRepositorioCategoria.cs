using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Evaluaciones.Repositorios;

public interface IRepositorioCategoria : IRepositorio<Categoria>
{
    Task<List<Categoria>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistePorNombreAsync(string nombre, Guid? excluirId, CancellationToken cancellationToken = default);
}
