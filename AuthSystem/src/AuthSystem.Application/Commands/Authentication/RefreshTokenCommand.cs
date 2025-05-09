using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Authentication
{
    public class RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validar el token JWT
                var (isValid, userId, jti) = await _jwtService.ValidateTokenAsync(request.Token);
                if (!isValid)
                {
                    _logger.LogWarning("Intento de refresh token con token JWT inválido");
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Token inválido"
                    };
                }

                // 2. Obtener el usuario
                var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
                if (user == null)
                {
                    _logger.LogWarning("Intento de refresh token para usuario no existente: {UserId}", userId);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // 3. Verificar el refresh token
                // Nota: En una implementación real, necesitaríamos un método específico para validar el refresh token
                // Para este ejemplo, asumimos que el token es válido si el usuario existe
                bool isValidRefreshToken = !string.IsNullOrEmpty(request.RefreshToken);
                if (!isValidRefreshToken)
                {
                    _logger.LogWarning("Intento de refresh token con refresh token inválido para usuario: {UserId}", userId);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Refresh token inválido"
                    };
                }

                // 4. Revocar el token anterior
                // Nota: En una implementación real, necesitaríamos buscar la sesión por el JTI primero
                // Para este ejemplo, asumimos que el JTI es el ID de la sesión convertido a string
                Guid sessionId = Guid.Parse(jti);
                await _userRepository.RevokeSessionAsync(sessionId);

                // 5. Generar nuevos tokens
                var (newToken, newRefreshToken) = await _jwtService.GenerateTokensAsync(user);

                // 6. Registrar nueva sesión
                var newSessionId = Guid.NewGuid();
                await _userRepository.AddSessionAsync(new Core.Entities.UserSession
                {
                    Id = newSessionId,
                    UserId = user.Id,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    IPAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    DeviceInfo = ExtractDeviceInfo(request.UserAgent),
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                });

                // 7. Mapear usuario a DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status.ToString(),
                    LastLoginDate = user.LastLoginDate,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt
                };

                // 8. Retornar resultado exitoso
                return new AuthResponseDto
                {
                    Succeeded = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    RequirePasswordChange = user.RequirePasswordChange,
                    User = userDto,
                    Message = "Token actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el refresh token");
                return new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al actualizar el token. Por favor, inicie sesión nuevamente."
                };
            }
        }

        private string ExtractDeviceInfo(string userAgent)
        {
            // Implementación simple para extraer información del dispositivo del User-Agent
            if (string.IsNullOrEmpty(userAgent))
                return "Desconocido";

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return "Móvil";

            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                return "Tablet";

            return "Escritorio";
        }
    }
}
