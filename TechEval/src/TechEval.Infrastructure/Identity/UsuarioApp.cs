using Microsoft.AspNetCore.Identity;

namespace TechEval.Infrastructure.Identity;

public sealed class UsuarioApp : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}

public sealed class RefreshTokenEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UsuarioId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public bool Revocado { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
