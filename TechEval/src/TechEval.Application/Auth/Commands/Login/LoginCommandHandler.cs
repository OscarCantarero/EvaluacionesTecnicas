using MediatR;
using TechEval.Application.Common.Exceptions;
using TechEval.Application.Common.Interfaces;

namespace TechEval.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler(IServicioAuth servicioAuth)
    : IRequestHandler<LoginCommand, TokenDto>
{
    public async Task<TokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var token = await servicioAuth.LoginAsync(command.Email, command.Password, cancellationToken)
            ?? throw new UnauthorizedException("Credenciales inválidas.");
        return token;
    }
}
