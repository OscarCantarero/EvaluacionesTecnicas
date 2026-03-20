using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Common.Errors;

public sealed class ErrorDominio(string codigo, string descripcion)
{
    public string Codigo { get; } = codigo;
    public string Descripcion { get; } = descripcion;

    public DomainException ToException() => new(this);
}

public sealed class DomainException(ErrorDominio error) : Exception(error.Descripcion)
{
    public ErrorDominio Error { get; } = error;
}
