using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Authentication
{
    public class LogoutCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string? Token { get; set; }
    }

    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LogoutCommandHandler> _logger;

        public LogoutCommandHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<LogoutCommandHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    // Si no se proporciona un token específico, revocar todas las sesiones del usuario
                    await _userRepository.RevokeAllUserSessionsAsync(request.UserId);
                    _logger.LogInformation("Todas las sesiones revocadas para el usuario {UserId}", request.UserId);
                    return true;
                }
                else
                {
                    // Validar el token JWT para obtener el JTI
                    var (isValid, userId, jti) = await _jwtService.ValidateTokenAsync(request.Token);
                    if (!isValid || userId != request.UserId.ToString())
                    {
                        _logger.LogWarning("Intento de logout con token JWT inválido para usuario {UserId}", request.UserId);
                        return false;
                    }

                    // Buscar la sesión por el JTI y revocarla
                    // Nota: En una implementación real, necesitaríamos buscar la sesión por el JTI primero
                    // Para este ejemplo, asumimos que el JTI es el ID de la sesión convertido a string
                    Guid sessionId = Guid.Parse(jti);
                    await _userRepository.RevokeSessionAsync(sessionId);
                    _logger.LogInformation("Sesión {Jti} revocada para el usuario {UserId}", jti, request.UserId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el logout para usuario {UserId}", request.UserId);
                return false;
            }
        }
    }
}
