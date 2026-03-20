namespace TechEval.Application.Common.DTOs;

public sealed record PaginacionParams(
    int Pagina = 1,
    int TamanoPagina = 10,
    string? Busqueda = null,
    string? Estado = null
)
{
    public int Skip => (Pagina - 1) * TamanoPagina;
}

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Items,
    int TotalItems,
    int Pagina,
    int TamanoPagina
)
{
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamanoPagina);
    public bool TieneSiguiente => Pagina < TotalPaginas;
    public bool TieneAnterior => Pagina > 1;
}
