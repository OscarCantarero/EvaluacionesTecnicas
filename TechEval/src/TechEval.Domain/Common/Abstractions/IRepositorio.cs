namespace TechEval.Domain.Common.Abstractions;

public interface IRepositorio<TAgregado> where TAgregado : AgregadoRaiz
{
    Task<TAgregado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(TAgregado agregado, CancellationToken cancellationToken = default);
    Task ActualizarAsync(TAgregado agregado, CancellationToken cancellationToken = default);
    Task EliminarAsync(TAgregado agregado, CancellationToken cancellationToken = default);
}
