namespace TechEval.Application.Common.Interfaces;

public interface IContextoUsuario
{
    string UsuarioId { get; }
    string Nombre { get; }
    bool EsEvaluador { get; }
    bool EsCandidato { get; }
    bool EsAdministrador { get; }
}
