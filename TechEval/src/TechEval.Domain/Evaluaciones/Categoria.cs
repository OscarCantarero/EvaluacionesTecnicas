using TechEval.Domain.Common.Abstractions;

namespace TechEval.Domain.Evaluaciones;

/// <summary>Categoría temática de preguntas (ej: "Backend .NET", "SQL", "Angular")</summary>
public sealed class Categoria : AgregadoRaiz
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public string CreadoPor { get; private set; } = string.Empty;
    public DateTime CreadoEn { get; private set; }
    public DateTime? ActualizadoEn { get; private set; }

    private Categoria() { }

    public static Categoria Crear(string nombre, string? descripcion, string creadoPor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        return new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Descripcion = descripcion?.Trim(),
            CreadoPor = creadoPor,
            CreadoEn = DateTime.UtcNow
        };
    }

    public void Actualizar(string nombre, string? descripcion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        ActualizadoEn = DateTime.UtcNow;
    }
}
