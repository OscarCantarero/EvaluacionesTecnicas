namespace TechEval.Application.Common.Interfaces;

public interface IServicioArchivos
{
    Task<string> GuardarAsync(Stream contenido, string nombreArchivo, string carpeta, CancellationToken cancellationToken = default);
    Task EliminarAsync(string ruta, CancellationToken cancellationToken = default);
    string ObtenerUrlPublica(string ruta);
}
