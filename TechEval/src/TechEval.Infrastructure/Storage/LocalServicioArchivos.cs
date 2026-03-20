using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Infrastructure.Storage;

public sealed class LocalServicioArchivos(IWebHostEnvironment env) : IServicioArchivos
{
    private readonly string _baseDir = Path.Combine(env.ContentRootPath, "uploads");

    public async Task<string> GuardarAsync(
        Stream contenido, string nombreArchivo, string carpeta,
        CancellationToken cancellationToken = default)
    {
        var dirDestino = Path.Combine(_baseDir, carpeta);
        Directory.CreateDirectory(dirDestino);

        var nombreSeguro = $"{Guid.NewGuid()}_{Path.GetFileName(nombreArchivo)}";
        var rutaCompleta = Path.Combine(dirDestino, nombreSeguro);

        await using var fs = new FileStream(rutaCompleta, FileMode.Create);
        await contenido.CopyToAsync(fs, cancellationToken);

        return Path.Combine(carpeta, nombreSeguro).Replace('\\', '/');
    }

    public Task EliminarAsync(string ruta, CancellationToken cancellationToken = default)
    {
        var rutaCompleta = Path.Combine(_baseDir, ruta);
        if (File.Exists(rutaCompleta))
            File.Delete(rutaCompleta);
        return Task.CompletedTask;
    }

    public string ObtenerUrlPublica(string ruta) => $"/uploads/{ruta.Replace('\\', '/')}";
}
