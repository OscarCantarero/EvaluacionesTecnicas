namespace TechEval.Application.Common.Exceptions;

public sealed class NotFoundException(string nombre, object clave)
    : Exception($"'{nombre}' con clave '{clave}' no fue encontrado.");

public sealed class ValidationException(IEnumerable<ValidationError> errores)
    : Exception("Ocurrieron uno o más errores de validación.")
{
    public IReadOnlyList<ValidationError> Errores { get; } = errores.ToList().AsReadOnly();
}

public sealed record ValidationError(string Propiedad, string Mensaje);

public sealed class ConflictException(string mensaje) : Exception(mensaje);

public sealed class UnauthorizedException(string mensaje) : Exception(mensaje);
