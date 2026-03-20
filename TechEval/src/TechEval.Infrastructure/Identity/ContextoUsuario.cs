using Microsoft.AspNetCore.Http;
using TechEval.Application.Common.Interfaces;
using System.Security.Claims;

namespace TechEval.Infrastructure.Identity;

public sealed class ContextoUsuario(IHttpContextAccessor httpContextAccessor) : IContextoUsuario
{
    private ClaimsPrincipal Usuario => httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No hay contexto HTTP disponible.");

    public string UsuarioId => Usuario.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Usuario.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Usuario no autenticado.");

    public string Nombre => Usuario.FindFirstValue(ClaimTypes.Name)
        ?? Usuario.FindFirstValue("name")
        ?? string.Empty;

    public bool EsEvaluador => Usuario.IsInRole("Evaluador");
    public bool EsCandidato => Usuario.IsInRole("Candidato");
    public bool EsAdministrador => Usuario.IsInRole("Administrador");
}
